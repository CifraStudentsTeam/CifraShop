namespace CifraShop.Contracts.Requests.Products
{
    public class BatchDeleteProductsRequest
    {
        public List<int> ProductIds { get; set; } = new();
    }
}
