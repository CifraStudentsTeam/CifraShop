using CifraShop.Client.Pages;
using CifraShop.Client.Services;
using CifraShop.Components;
using CifraShop.Components.Controllers;
using CifraShop.Data.Additionally;
using CifraShop.Data.AppDbContext;
using CIfraShop.Services.Implementations;
using CIfraShop.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
// 2. Регистрируем наш ApiService (ОБЯЗАТЕЛЬНО!)
builder.Services.AddScoped<ApiService>();

// 3. Регистрируем UserState (зависит от ApiService)
builder.Services.AddScoped<UserState>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddDbContext<ApplicationContext>(options =>
          options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<UserState>();
// В файле Program.cs


// Регистрация контроллеров как сервисов
builder.Services.AddControllersWithViews()
       .AddControllersAsServices();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    dbContext.Database.Migrate(); // создаёт БД и применяет миграции
    DbSeeder.Seed(dbContext);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();
app.MapControllers();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CifraShop.Client._Imports).Assembly);

app.Run();
