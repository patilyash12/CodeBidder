using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CodeBidder.Data;
using System;

var builder = WebApplication.CreateBuilder(args);

// Register SQL Server DbContext with additional configurations (timeout and error resiliency)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions
            .CommandTimeout(60) // Set command timeout to 60 seconds
            .EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null) // Enable retry on transient failure
    ));

// Add support for controllers and views (MVC)
builder.Services.AddControllersWithViews();

// Enable session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Add session middleware before using sessions in controllers
app.UseSession(); // <-- This ensures session is available for requests

app.UseAuthorization();

// Map default controller route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
