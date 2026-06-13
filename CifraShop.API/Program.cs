using CifraShop.Application.Services.Implementations;
using CifraShop.Application.Services.Interfaces;
using CifraShop.Infrastructure.Data.Repositories.Implementations;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepositoryEfCore>();
builder.Services.AddScoped<IOrderRepository, OrderRepositoryEfCore>();
builder.Services.AddScoped<IProductRepository, ProductRepositoryEfCore>();
builder.Services.AddScoped<IUserRepository, UserRepositoryEfCore>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
