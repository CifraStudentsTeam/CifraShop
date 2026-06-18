namespace CifraShop.Contracts.Responses.OrderItem
{
    public class UpdateOrderItemResponse
    {
        public int Id { get; set; }
        public short Quantity { get; set; }
        public short? Price { get; set; }
    }
}
