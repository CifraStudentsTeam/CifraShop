namespace CifraShop.Contracts.Requests.OrderItem
{
    public class UpdateOrderItemRequest
    {
        public int Id { get; set; }
        public short Quantity { get; set; }
        public short? Price { get; set; }
    }
}
