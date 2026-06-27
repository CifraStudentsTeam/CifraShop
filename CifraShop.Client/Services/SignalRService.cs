using Microsoft.AspNetCore.SignalR.Client;

namespace CifraShop.Client.Services;

public class SignalRService : IAsyncDisposable
{
    private HubConnection? _connection;
    private readonly string _hubUrl;
    private bool _disposed;

    public event Action<string, string>? OnNotify;
    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public SignalRService(string hubUrl = "http://localhost:5000/hubs/admin")
    {
        _hubUrl = hubUrl;
    }

    public async Task StartAsync()
    {
        if (_disposed) return;

        if (_connection != null)
        {
            try { await _connection.DisposeAsync(); } catch { }
            _connection = null;
        }

        _connection = new HubConnectionBuilder()
            .WithUrl(_hubUrl)
            .WithAutomaticReconnect(new[]
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30)
            })
            .Build();

        _connection.On<string, string>("Notify", (entity, action) =>
        {
            OnNotify?.Invoke(entity, action);
        });

        _connection.Reconnecting += _ =>
        {
            Console.WriteLine("[SignalR] Переподключение...");
            return Task.CompletedTask;
        };

        _connection.Reconnected += _ =>
        {
            Console.WriteLine("[SignalR] Подключено");
            return Task.CompletedTask;
        };

        _connection.Closed += async _ =>
        {
            if (_disposed) return;
            Console.WriteLine("[SignalR] Соединение закрыто. Переподключение через 5 сек...");
            await Task.Delay(5000);
            try { await StartAsync(); } catch { }
        };

        for (int attempt = 1; attempt <= 5; attempt++)
        {
            try
            {
                await _connection.StartAsync();
                Console.WriteLine("[SignalR] Подключено");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR] Попытка {attempt}/5 не удалась: {ex.Message}");
                if (attempt < 5) await Task.Delay(2000 * attempt);
            }
        }
        Console.WriteLine("[SignalR] Не удалось подключиться после 5 попыток");
    }

    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        if (_connection != null)
        {
            try { await _connection.DisposeAsync(); } catch { }
        }
    }
}
