using Microsoft.AspNetCore.SignalR.Client;

namespace CifraShop.Client.Services;

public class ShopSignalRService : IAsyncDisposable
{
    private HubConnection? _connection;
    private readonly string _hubUrl;

    public event Action<string, string>? OnNotify;
    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public ShopSignalRService(string hubUrl = "http://localhost:5000/hubs/shop")
    {
        _hubUrl = hubUrl;
    }

    public async Task StartAsync()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(_hubUrl)
            .WithAutomaticReconnect(new[]
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            })
            .Build();

        _connection.On<string, string>("Notify", (entity, action) =>
        {
            OnNotify?.Invoke(entity, action);
        });

        _connection.Reconnecting += _ =>
        {
            Console.WriteLine("[ShopSignalR] Переподключение...");
            return Task.CompletedTask;
        };

        _connection.Reconnected += _ =>
        {
            Console.WriteLine("[ShopSignalR] Подключено");
            return Task.CompletedTask;
        };

        _connection.Closed += async _ =>
        {
            Console.WriteLine("[ShopSignalR] Соединение закрыто. Переподключение через 5 сек...");
            await Task.Delay(5000);
            if (_connection != null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
            try { await StartAsync(); } catch { }
        };

        await _connection.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
    }
}