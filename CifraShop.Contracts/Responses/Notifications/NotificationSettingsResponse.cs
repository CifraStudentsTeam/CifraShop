namespace CifraShop.Contracts.Responses.Notifications
{
    public class NotificationSettingsResponse
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string? AdminEmails { get; set; }
        public bool NotifyOnNewOrder { get; set; }
        public bool NotifyOnStatusChange { get; set; }
        public bool NotifyOnLowStock { get; set; }
        public int LowStockThreshold { get; set; }
    }
}
