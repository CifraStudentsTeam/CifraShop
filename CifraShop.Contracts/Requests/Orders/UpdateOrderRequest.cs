using CifraShop.Domain.Enums;

namespace CifraShop.Contracts.Requests.Orders
{
    public class UpdateOrderRequest
    {
        public StatusOrder? Status { get; set; }
    }
}
