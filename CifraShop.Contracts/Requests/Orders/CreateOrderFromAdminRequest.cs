namespace CifraShop.Contracts.Requests.Orders
{
    public class CreateOrderFromAdminRequest
    {
        public string CustomerLogin { get; set; } = string.Empty;
        public List<Requests.OrderItem.OrderItemRequest> Items { get; set; } = new();
    }
}
