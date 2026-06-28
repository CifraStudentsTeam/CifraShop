namespace CifraShop.Client.Models;

/// <summary>Модель товара, соответствующая ProductResponse с API</summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int Price { get; set; }
    public int Quantity { get; set; }
    public StatusProduct Status { get; set; }
    public string? ImageUrl { get; set; }
    public string Branch { get; set; } = "";
    public bool IsSelected { get; set; }
}

/// <summary>Модель позиции заказа, соответствующая OrderItemResponse с API</summary>
public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int Price { get; set; }
    public int Quantity { get; set; }
    public int Total => Price * Quantity;
}

/// <summary>Модель заказа, соответствующая OrderResponse с API</summary>
public class OrderDto
{
    public int Id { get; set; }
    public StatusOrder Status { get; set; }
    public bool IsSelected { get; set; }
    public int Sum { get; set; }
    public DateTime DateOfPurchase { get; set; }
    public string CustomerEmail { get; set; } = "";
    public int CustomerId { get; set; }
    public string? ImageUrl { get; set; }
    public string Branch { get; set; } = "";
    public List<OrderItemDto> Items { get; set; } = new();
}

/// <summary>Модель пользователя, соответствующая UserResponse с API</summary>
public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public int? Balance { get; set; }
    public UserRole Role { get; set; }
    public string? Branch { get; set; }
}

/// <summary>Модель действия администратора</summary>
public class AdminActionDto
{
    public int Id { get; set; }
    public string ActionType { get; set; } = "";
    public string Details { get; set; } = "";
    public string Branch { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

/// <summary>Модель изображения товара</summary>
public class ProductImageDto
{
    public int Id { get; set; }
    public string Url { get; set; } = "";
    public string FileName { get; set; } = "";
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>Модель изображения заказа</summary>
public class OrderImageDto
{
    public int Id { get; set; }
    public string Url { get; set; } = "";
    public string FileName { get; set; } = "";
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>Модель настроек уведомлений для филиала</summary>
public class NotificationSettingsDto
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string TelegramBotToken { get; set; } = "";
    public string TelegramChatId { get; set; } = "";
    public string Branch { get; set; } = "";
    public string AdminEmails { get; set; } = "";
    public bool NotifyOnNewOrder { get; set; }
    public bool NotifyOnStatusChange { get; set; }
    public bool NotifyOnLowStock { get; set; }
    public int LowStockThreshold { get; set; }
}
