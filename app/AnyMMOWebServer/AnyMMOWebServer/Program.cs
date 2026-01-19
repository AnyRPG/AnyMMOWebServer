using AnyMMOWebServer.Models;
using AnyMMOWebServer.Services;
using AnyMMOWebServer.Database;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// bind configuration data to settings class and add it for dependency injection
var settings = new AnyMMOWebServerSettings();
builder.Configuration.Bind("AnyMMOWebServerSettings", settings);
if (builder.Environment.IsProduction()) {
    settings.BearerKey = builder.Configuration["BearerKey"];
}
builder.Services.AddSingleton(settings);

// configure logging - console for local, lambda logger for production
if (builder.Environment.IsProduction()) {
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
} else {
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
}

// use mysql
if (builder.Environment.IsProduction()) {
    builder.Services.AddDbContext<GameDbContext>(o =>
        //o.UseMySQL(builder.Configuration["DatabaseConnectionString"])
        o.UseMySQL(builder.Configuration.GetConnectionString("Db"))
    );
} else {
    builder.Services.AddDbContext<GameDbContext>(o =>
        o.UseMySQL(builder.Configuration.GetConnectionString("Db"))
    );
}

// setup controllers
builder.Services.AddControllersWithViews();

// make authentication service available to dependency injection
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// configure site to use jwt for authentication
/*
builder.Services.AddAuthentication(opt => {
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        //ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        //ValidIssuer = ConfigurationManager.AppSetting["JWT:ValidIssuer"],
        //ValidAudience = ConfigurationManager.AppSetting["JWT:ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(settings.BearerKey))
    };
});
*/

// configure site to use cookies for authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options => {
                options.LoginPath = "/Home";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
                options.SlidingExpiration = true;
            })
            .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    //ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    //ValidIssuer = ConfigurationManager.AppSetting["JWT:ValidIssuer"],
                    //ValidAudience = ConfigurationManager.AppSetting["JWT:ValidAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(settings.BearerKey))
                };
            });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();