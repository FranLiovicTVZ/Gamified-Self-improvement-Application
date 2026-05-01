using GamefiedSelfImprovement;
using Gamified_Self_Improvement.Repositories;
using Microsoft.EntityFrameworkCore;

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

var app = builder.Build();

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
