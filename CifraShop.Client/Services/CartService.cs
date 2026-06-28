using System.Net.Http.Json;

namespace CifraShop.Client.Services;

public class CartService
{
    private readonly HttpClient _http;
    private readonly List<CartItem> _items = new();

    public CartService(HttpClient http)
    {
        _http = http;
    }

    public event Action? OnCartChanged;
    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();
    public int ItemsCount => _items.Sum(i => i.Quantity);
    public int TotalSum => _items.Sum(i => i.Price * i.Quantity);
    public bool IsEmpty => !_items.Any();

    public void AddItem(Models.ProductDto product, int quantity = 1)
    {
        var existing = _items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            _items.Add(new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = product.ImageUrl
            });
        }
        OnCartChanged?.Invoke();
    }

    public void RemoveItem(int productId)
    {
        _items.RemoveAll(i => i.ProductId == productId);
        OnCartChanged?.Invoke();
    }

    public void UpdateQuantity(int productId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            if (quantity <= 0)
                _items.Remove(item);
            else
                item.Quantity = quantity;
            OnCartChanged?.Invoke();
        }
    }

    public void Clear()
    {
        _items.Clear();
        OnCartChanged?.Invoke();
    }

    public async Task<(bool success, string? error)> CheckoutAsync(string customerEmail)
    {
        if (IsEmpty)
            return (false, "Корзина пуста");

        try
        {
            var request = new
            {
                CustomerEmail = customerEmail,
                Items = _items.Select(i => new { ProductId = i.ProductId, Quantity = i.Quantity }).ToList()
            };

            var response = await _http.PostAsJsonAsync("api/Order/create-order", request);

            if (response.IsSuccessStatusCode)
            {
                Clear();
                return (true, null);
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrEmpty(error) ? "Ошибка оформления заказа" : error);
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка соединения: {ex.Message}");
        }
    }
}

public class CartItem
{
    public int ProductId { get; set; }
    public string Name { get; set; } = "";
    public int Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public int Total => Price * Quantity;
}