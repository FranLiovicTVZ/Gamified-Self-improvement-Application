using GamefiedSelfImprovement;
using Gamified_Self_Improvement.Repositories;
using Gamified_Self_Improvement.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;
using Serilog.Events;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/app-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
});

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "GamifiedSelfImprovementTestKeys")));
}

// Add services to the container
builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddHttpClient();

// MCP Server - exposes app data to agentic IDEs (Claude Code, Cursor, etc.)
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<GamefiedSelfImprovement.McpTools.ActivityTools>()
    .WithTools<GamefiedSelfImprovement.McpTools.UserTools>()
    .WithTools<GamefiedSelfImprovement.McpTools.StreakTools>()
    .WithTools<GamefiedSelfImprovement.McpTools.SearchTools>()
    .WithTools<GamefiedSelfImprovement.McpTools.BookTools>();

// Configure DbContext for Entity Framework with Identity support
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<GamefiedSelfImprovementDbContext>(options =>
        options.UseInMemoryDatabase("GamifiedSelfImprovementTests")
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
}
else if (builder.Environment.IsProduction())
{
    var dbDir = Path.Combine(builder.Environment.ContentRootPath, "data");
    Directory.CreateDirectory(dbDir);
    var dbPath = Path.Combine(dbDir, "gamified.db");
    builder.Services.AddDbContext<GamefiedSelfImprovementDbContext>(options =>
        options.UseSqlite($"Data Source={dbPath}")
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
}
else
{
    builder.Services.AddDbContext<GamefiedSelfImprovementDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("GamefiedSelfImprovementDbContext"))
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
}

// Configure Identity with AppUser and roles
builder.Services
    .AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequiredLength = 5;
    })
    .AddEntityFrameworkStores<GamefiedSelfImprovementDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/auth/login";
    options.LogoutPath = "/auth/logout";
    options.AccessDeniedPath = "/auth/login";
});

var authSection = builder.Configuration.GetSection("Authentication");
var googleClientId = authSection["Google:ClientId"];
var googleClientSecret = authSection["Google:ClientSecret"];
var facebookAppId = authSection["Facebook:AppId"];
var facebookAppSecret = authSection["Facebook:AppSecret"];

// Configure OAuth authentication (SignInScheme mora biti External za Identity)
var authBuilder = builder.Services.AddAuthentication();
if (IsConfigured(googleClientId) && IsConfigured(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId!;
        options.ClientSecret = googleClientSecret!;
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.CallbackPath = "/signin-google";
    });
}

if (IsConfigured(facebookAppId) && IsConfigured(facebookAppSecret))
{
    authBuilder.AddFacebook(options =>
    {
        options.AppId = facebookAppId!;
        options.AppSecret = facebookAppSecret!;
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.CallbackPath = "/signin-facebook";
    });
}

// Register EF Repositories and services
builder.Services.AddScoped<UserSyncService>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ActivityRepository>();

// Keep mock repositories for backward compatibility if needed
builder.Services.AddSingleton<UserMockRepository>();
builder.Services.AddSingleton<ActivityMockRepository>();
builder.Services.AddSingleton<GameDatabase>();

// Add Razor Pages support
builder.Services.AddRazorPages();

// Configure localization for date time formatting (hr-HR and en-US)
var supportedCultures = new[]
{
    new CultureInfo("hr-HR"),
    new CultureInfo("en-US")
};

var app = builder.Build();

// Test okruženje: InMemory baza i uloge
if (app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<GamefiedSelfImprovementDbContext>();
        await db.Database.EnsureCreatedAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedRolesAsync(roleManager);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        await SeedAdminUserAsync(userManager);
    }
}

// Apply migrations and seed data at startup (except in Testing environment)
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<GamefiedSelfImprovementDbContext>();
        
        try
        {
            if (app.Environment.IsProduction())
            {
                // EnsureCreated ne kreira tablice ako datoteka već postoji bez tablica
                // (ostaci od neuspjele SQL Server migracije) — briši i kreiraj svježe
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();
            }
            else
            {
                await db.Database.MigrateAsync();
            }
            Console.WriteLine("Database schema applied successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Migration error: {ex.Message}");
        }
        
        // Seed roles and admin user
        try
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            
            await SeedRolesAsync(roleManager);
            await SeedAdminUserAsync(userManager);

            var userSync = scope.ServiceProvider.GetRequiredService<UserSyncService>();
            var admin = await userManager.FindByEmailAsync("admin@gamified.hr");
            if (admin != null)
            {
                await userSync.SyncFromAppUserAsync(admin);
            }
        }
        catch (Exception ex)
        {
            // Log seeding errors but don't crash
            Console.WriteLine($"Seeding error: {ex.Message}");
        }
    }
}

// Configure request localization
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("hr-HR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Dashboard}/{id?}");

app.MapRazorPages();

// MCP endpoint — connect Claude Code with: /mcp add gamified http://localhost:5000/mcp/sse
app.MapMcp("/mcp");

app.Run();

// Seed roles
async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
{
    string[] roles = { "Admin", "Manager", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Seed admin user
async Task SeedAdminUserAsync(UserManager<AppUser> userManager)
{
    var adminUser = await userManager.FindByEmailAsync("admin@gamified.hr");
    if (adminUser == null)
    {
        var user = new AppUser
        {
            UserName = "admin",
            Email = "admin@gamified.hr",
            OIB = "12345678901",
            JMBG = "1234567890123",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, "Admin123");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, "Admin");
    }

    var gmailAdmin = await userManager.FindByEmailAsync("admin@gmail.com");
    if (gmailAdmin == null)
    {
        var user = new AppUser
        {
            UserName = "admin_gmail",
            Email = "admin@gmail.com",
            OIB = "12345678902",
            JMBG = "1234567890124",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, "admin");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, "Admin");
    }
}

static bool IsConfigured(string? value)
{
    return !string.IsNullOrWhiteSpace(value) &&
           !value.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase);
}

public partial class Program { }
