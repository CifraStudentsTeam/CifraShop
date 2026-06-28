using System.Text;
using CifraShop.API;
using CifraShop.API.Hubs;
using CifraShop.API.Middleware;
using CifraShop.Application.Services.Implementations;
using CifraShop.Application.Services.Interfaces;
using CifraShop.Domain.Auth;
using CifraShop.Infrastructure.Auth;
using CifraShop.Infrastructure.Data;
using CifraShop.Application.Models;
using CifraShop.Infrastructure.Services;
using CifraShop.Domain.Repositories;
using CifraShop.Infrastructure.Data.Repositories.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = null;
    o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddSignalR();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderItemRepository, OrderItemRepositoryEfCore>();
builder.Services.AddScoped<IOrderRepository, OrderRepositoryEfCore>();
builder.Services.AddScoped<IProductRepository, ProductRepositoryEfCore>();
builder.Services.AddScoped<IUserRepository, UserRepositoryEfCore>();
builder.Services.AddScoped<IAdminActionRepository, AdminActionRepositoryEfCore>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepositoryEfCore>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminActionService, AdminActionService>();
builder.Services.AddScoped<INotificationSettingsRepository, NotificationSettingsRepositoryEfCore>();
builder.Services.AddScoped<INotificationSettingsService, NotificationSettingsService>();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddScoped<NotificationDispatcher>();
builder.Services.AddScoped<IOrderImageRepository, OrderImageRepositoryEfCore>();
builder.Services.AddScoped<IOrderImageService, OrderImageService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key не настроен в конфигурации");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "CifraShop",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "CifraShop",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientCORS",
        policy =>
        {
            policy.WithOrigins("https://localhost:5001", "http://localhost:5001")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

DatabaseInitializer.ApplyMigrationsAsync(app);

app.UseStaticFiles();

app.UseCors("ClientCORS");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Ok("OK"));

app.MapHub<AdminHub>("/hubs/admin");

app.MapHub<ShopHub>("/hubs/shop");

app.Run();

