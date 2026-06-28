using CifraShop.Client;
using CifraShop.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5000/")
});

builder.Services.AddScoped<SignalRService>();
builder.Services.AddScoped<ShopSignalRService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CartService>();

await builder.Build().RunAsync();
