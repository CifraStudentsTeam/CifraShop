using CifraShop.Domain.Enums;

namespace CifraShop.Contracts.Requests.Products
{
    public class BatchUpdateProductStatusRequest
    {
        public List<int> ProductIds { get; set; } = new();
        public StatusProduct NewStatus { get; set; }
    }
}
