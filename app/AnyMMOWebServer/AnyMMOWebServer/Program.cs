using AnyMMOWebServer.Database;
using AnyMMOWebServer.Models;
using AnyMMOWebServer.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<ForwardedHeadersOptions>(options => {
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = null; // Allows any number of hops
    // If Nginx is NOT on the same machine, you may need to clear KnownProxies 
    // or add your Nginx IP to the list for it to be trusted.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Any, 0));
    options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.IPv6Any, 0));
});

// bind configuration data to settings class and add it for dependency injection
var settings = new AnyMMOWebServerSettings();
builder.Configuration.Bind("AnyMMOWebServerSettings", settings);
//if (builder.Environment.IsProduction()) {
    //settings.BearerKey = builder.Configuration["BearerKey"];
//}
builder.Services.AddSingleton(settings);

// configure logging - console for local, lambda logger for production
if (builder.Environment.IsProduction()) {
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
} else {
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
}

var connectionString = builder.Configuration.GetConnectionString("Db")
    ?? throw new InvalidOperationException("Connection string 'Db' not found.");

// use mysql
if (builder.Environment.IsProduction()) {
    builder.Services.AddDbContext<GameDbContext>(o =>
        //o.UseMySQL(builder.Configuration["DatabaseConnectionString"])
        o.UseMySQL(connectionString)
    );
} else {
    builder.Services.AddDbContext<GameDbContext>(o =>
        o.UseMySQL(connectionString)
    );
}

if (builder.Environment.IsProduction()) {
    // Persist keys to a specific directory within the container
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(@"/app/keys/"));
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
builder.Services.AddAuthentication()
            .AddCookie(options => {
                options.LoginPath = "/Home";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
                options.SlidingExpiration = true;
            })
            .AddJwtBearer(options => {
                options.TokenHandlers.Clear();
                options.TokenHandlers.Add(new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler());

                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    //ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    //ValidIssuer = ConfigurationManager.AppSetting["JWT:ValidIssuer"],
                    //ValidAudience = ConfigurationManager.AppSetting["JWT:ValidAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(settings.BearerKey))
                };
                options.Events = new JwtBearerEvents {
                    OnMessageReceived = context => {
                        string authHeader = context.Request.Headers["Authorization"].ToString();
                        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) {
                            // Manually trim and assign the token to bypass parsing errors
                            context.Token = authHeader.Substring("Bearer ".Length).Trim();
                        }
                        return Task.CompletedTask;
                    }
                };
            });

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.Use((context, next) => {
    var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
    // If this is empty, Nginx isn't sending the header at all.
    //Console.WriteLine($"Raw X-Forwarded-For: {forwardedFor}");
    return next();
});

app.UseForwardedHeaders(); // Must be first in the pipeline

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