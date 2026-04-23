using Microsoft.EntityFrameworkCore;
using EscalationAnalysisDb2.Infrastructure.Data;
using EscalationAnalysisDb2.Application.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Servicios
builder.Services.AddScoped<UploadService>();
builder.Services.AddScoped<CaseService>();
builder.Services.AddScoped<UserService>(); //  FALTABA

// DB
builder.Services.AddDbContext<EscalationsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔐 AUTHENTICATION (CLAVE)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔐 ORDEN CORRECTO
app.UseAuthentication();   // 🔥 FALTABA
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();



/*

using Microsoft.EntityFrameworkCore;
using EscalationAnalysisDb2.Infrastructure.Data;
using EscalationAnalysisDb2.Application.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<UploadService>();
builder.Services.AddScoped<CaseService>();

builder.Services.AddDbContext<EscalationsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

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
    pattern: "{controller=Upload}/{action=Index}/{id?}");

app.Run();
*/