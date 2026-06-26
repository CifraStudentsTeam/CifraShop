using Microsoft.AspNetCore.SignalR;

namespace CifraShop.API.Hubs;

/// <summary>
/// SignalR хаб для клиентской части магазина
/// Уведомляет о изменениях товаров и заказов в реальном времени
/// </summary>
public class ShopHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "ShopClients");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ShopClients");
        await base.OnDisconnectedAsync(exception);
    }
}