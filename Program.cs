using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Auth.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Load Configuration from appsettings.json
IConfiguration configuration = builder.Configuration;

// 🔹 Register Services
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 🔹 Add IHttpContextAccessor for session handling
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// 🔹 Register ADO.NET Database Helper (Pass Configuration)
builder.Services.AddSingleton<DbHelper>(new DbHelper(configuration));

// 🔹 Register Authentication & Email Services
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<EmailService>();

var app = builder.Build();

// 🔹 Enable Middleware
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultControllerRoute();
app.Run();
