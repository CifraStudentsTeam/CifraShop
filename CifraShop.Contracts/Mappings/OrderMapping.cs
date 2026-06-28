using CifraShop.Contracts.Responses.Orders;
using CifraShop.Domain.Entities;

namespace CifraShop.Contracts.Mappings
{
    public static class OrderMapping
    {
        public static OrderResponse ToResponse(this Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                Status = order.Status,
                Sum = order.Sum,
                DateOfPurchase = order.DateOfPurchase,
                CustomerEmail = order.Customer?.Email ?? "",
                CustomerId = order.CustomerId,
                Items = order.OrderItems?.Select(i => i.ToResponse()).ToList() ?? new(),
                Branch = order.Branch
            };
        }
    }
}
