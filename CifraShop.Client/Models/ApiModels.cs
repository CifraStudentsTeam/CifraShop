namespace CifraShop.Client.Models;

/// <summary>Модель товара, соответствующая ProductResponce с API</summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public short Price { get; set; }
    public short Quantity { get; set; }
    public int Status { get; set; }
    public bool IsSelected { get; set; }
}

/// <summary>Модель позиции заказа, соответствующая OrderItemResponse с API</summary>
public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public short Price { get; set; }
    public short Quantity { get; set; }
    public int Total => Price * Quantity;
}

/// <summary>Модель заказа, соответствующая OrderResponse с API</summary>
public class OrderDto
{
    public int Id { get; set; }
    public int Status { get; set; }
    public bool IsSelected { get; set; }
    public short Sum { get; set; }
    public DateTime DateOfPurchase { get; set; }
    public string CustomerLogin { get; set; } = "";
    public int CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

/// <summary>Модель пользователя, соответствующая UserResponse с API</summary>
public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public short? Balance { get; set; }
    public int Role { get; set; }
}
