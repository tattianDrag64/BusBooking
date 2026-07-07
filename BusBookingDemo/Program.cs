using BusBookingDemo.Data;
using BusBookingDemo.Entity;
using BusBookingDemo.Repository;
using BusBookingDemo.Repository.IRepositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Users/SignIn"; // Путь для страницы входа
        options.LogoutPath = "/Users/Logout"; // Путь для выхода
        options.AccessDeniedPath = "/Users/AccessDenied"; // Путь для доступа с ограничениями
    });

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddRoles<IdentityRole>()
//    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Trip}/{action=Index}/{id?}");
//pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");


app.Run();
