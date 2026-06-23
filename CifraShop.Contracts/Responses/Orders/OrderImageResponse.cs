namespace CifraShop.Contracts.Responses.Orders
{
    public class OrderImageResponse
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }
}
