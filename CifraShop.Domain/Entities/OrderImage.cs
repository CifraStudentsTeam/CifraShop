namespace CifraShop.Domain.Entities
{
    public class OrderImage
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public string FileName { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }
}
