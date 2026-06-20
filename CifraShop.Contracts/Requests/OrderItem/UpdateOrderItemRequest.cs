namespace CifraShop.Contracts.Requests.OrderItem
{
    public class UpdateOrderItemRequest
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int? Price { get; set; }
    }
}
