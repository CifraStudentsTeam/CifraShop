using CifraShop.Contracts.Responses.Orders;
using CifraShop.Domain.Entities;

namespace CifraShop.Contracts.Mappings
{
    public static class OrderMapping
    {
        public static OrderResponse ToResponse(this Order order, List<OrderItem> items)
        {
            return new OrderResponse
            {
                Id = order.Id,
                Status = order.Status,
                Sum = order.Sum,
                DateOfPurchase = order.DateOfPurchase,
                CustomerLogin = order.CustomerLogin,
                CustomerId = order.CustomerId,
                Items = items.Select(i => i.ToResponse()).ToList()
            };
        }
    }
}
