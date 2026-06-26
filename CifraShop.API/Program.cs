using CifraShop.API.Hubs;
using CifraShop.API.Middleware;
using CifraShop.Application.Services.Implementations;
using CifraShop.Application.Services.Interfaces;
using CifraShop.Infrastructure.Data;
using CifraShop.Domain.Repositories;
using CifraShop.Infrastructure.Data.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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
builder.Services.AddScoped<IOrderImageRepository, OrderImageRepositoryEfCore>();
builder.Services.AddScoped<IOrderImageService, OrderImageService>();

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

_ = Task.Run(async () =>
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    for (int attempt = 1; attempt <= 15; attempt++)
    {
        try
        {
            db.Database.Migrate();
            Console.WriteLine("[API] БД обновлена");
            return;
        }
        catch (Exception ex) when (attempt < 15)
        {
            Console.WriteLine($"[API] БД недоступна (попытка {attempt}/15): {ex.Message}. Повтор через 2 сек...");
            await Task.Delay(2000);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] Не удалось подключиться к БД после 15 попыток: {ex.Message}");
        }
    }
});

app.UseStaticFiles();

app.UseCors("ClientCORS");

app.UseAuthorization();

app.MapControllers();

app.MapHub<AdminHub>("/hubs/admin");

app.MapHub<ShopHub>("/hubs/shop");

app.Run();