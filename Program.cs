using GamefiedSelfImprovement;
using Gamified_Self_Improvement.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Register Mock Repositories for Dependency Injection
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
