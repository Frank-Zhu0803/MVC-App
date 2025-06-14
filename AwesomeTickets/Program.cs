using System.Configuration;
using AwesomeTickets.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using MySql.EntityFrameworkCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Razor Pages but with a lower order to give MVC priority
builder.Services.AddRazorPages(options => 
{
    options.RootDirectory = "/Pages";
    options.Conventions.AddPageRoute("/Index", "/razor-pages");
});

// Configure MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(connectionString)
);

// Add authentication services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Unauthorized";
        options.LogoutPath = "/Home/Logout";
        options.AccessDeniedPath = "/Home/Unauthorized";
    });

var app = builder.Build();

// Seed the database with an admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    
    if (!context.Users.Any())
    {
        context.Users.Add(new User
        {
            Username = "admin",
            Password = "admin123",
            Role = "Admin"
        });
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Add a direct redirect from root to Browse/Index
app.MapGet("/", context => {
    context.Response.Redirect("/Browse/Index");
    return Task.CompletedTask;
});

// Configure endpoints - MVC routes first, then Razor Pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Browse}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
