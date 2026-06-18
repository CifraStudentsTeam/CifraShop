namespace CifraShop.Contracts.Requests.Orders
{
    public class BatchUpdateOrderStatusRequest
    {
        public List<int> OrderIds { get; set; } = new();
        public int NewStatus { get; set; }
    }
}
