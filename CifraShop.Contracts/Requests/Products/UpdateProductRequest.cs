namespace CifraShop.Contracts.Requests.Products
{
    public class UpdateProductRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public short? Price { get; set; }
        public short? Quantity { get; set; }
        public int? Status { get; set; }
    }
}
