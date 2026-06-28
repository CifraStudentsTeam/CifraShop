namespace CifraShop.Domain.Entities
{
    public class NotificationSettings
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string? AdminEmails { get; set; }
        public bool NotifyOnNewOrder { get; set; } = true;
        public bool NotifyOnStatusChange { get; set; } = true;
        public bool NotifyOnLowStock { get; set; } = true;
        public int LowStockThreshold { get; set; } = 5;
    }
}
