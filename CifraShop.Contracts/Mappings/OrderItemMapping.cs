using CifraShop.Contracts.Responses.OrderItem;
using CifraShop.Domain.Entities;

namespace CifraShop.Contracts.Mappings
{
    public static class OrderItemMapping
    {
        public static OrderItemResponse ToResponse(this OrderItem item)
        {
            return new OrderItemResponse
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? string.Empty,
                Price = item.Price,
                Quantity = item.Quantity
            };
        }
    }
}
