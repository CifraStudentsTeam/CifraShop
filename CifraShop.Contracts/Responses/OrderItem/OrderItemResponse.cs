namespace CifraShop.Contracts.Responses.OrderItem
{
    public class OrderItemResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public short Price { get; set; }
        public short Quantity { get; set; }
        public int Total => Price * Quantity;
    }
}
