using CifraShop.API.Middleware;
using CifraShop.Application.Services.Implementations;
using CifraShop.Application.Services.Interfaces;
using CifraShop.Infrastructure.Data;
using CifraShop.Domain.Repositories;
using CifraShop.Infrastructure.Data.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderItemRepository, OrderItemRepositoryEfCore>();
builder.Services.AddScoped<IOrderRepository, OrderRepositoryEfCore>();
builder.Services.AddScoped<IProductRepository, ProductRepositoryEfCore>();
builder.Services.AddScoped<IUserRepository, UserRepositoryEfCore>();
builder.Services.AddScoped<IAdminActionRepository, AdminActionRepositoryEfCore>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminActionService, AdminActionService>();
builder.Services.AddScoped<INotificationSettingsRepository, NotificationSettingsRepositoryEfCore>();
builder.Services.AddScoped<INotificationSettingsService, NotificationSettingsService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientCORS",
        policy =>
        {
            policy.WithOrigins("https://localhost:5001", "http://localhost:5001")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.UseStaticFiles();

app.UseCors("ClientCORS");

app.MapControllers();

app.Run();
