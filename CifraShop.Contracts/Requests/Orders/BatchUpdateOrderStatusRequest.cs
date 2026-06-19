using CifraShop.Domain.Enums;

namespace CifraShop.Contracts.Requests.Orders
{
    public class BatchUpdateOrderStatusRequest
    {
        public List<int> OrderIds { get; set; } = new();
        public StatusOrder NewStatus { get; set; }
    }
}
