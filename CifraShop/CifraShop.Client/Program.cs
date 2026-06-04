using CifraShop.Client;
using CifraShop.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using static System.Net.Http.HttpClient;
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 1. Регистрируем HttpClient с базовым адресом API
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7001") // порт твоего API
});

// 2. Регистрируем наш ApiService (ОБЯЗАТЕЛЬНО!)
builder.Services.AddScoped<ApiService>();

// 3. Регистрируем UserState (зависит от ApiService)
builder.Services.AddScoped<UserState>();

await builder.Build().RunAsync();