using GamefiedSelfImprovement;
using Gamified_Self_Improvement.Repositories;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure DbContext for Entity Framework
builder.Services.AddDbContext<GamefiedSelfImprovementDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("GamefiedSelfImprovementDbContext")));

// Register EF Repositories for Dependency Injection
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ActivityRepository>();

// Keep mock repositories for backward compatibility if needed
builder.Services.AddSingleton<UserMockRepository>();
builder.Services.AddSingleton<ActivityMockRepository>();
builder.Services.AddSingleton<GameDatabase>();

// Configure localization for date time formatting (hr-HR and en-US)
var supportedCultures = new[]
{
    new CultureInfo("hr-HR"),
    new CultureInfo("en-US")
};

var app = builder.Build();

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
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Dashboard}/{id?}");

app.Run();
