namespace CifraShop.Contracts.Requests.Notifications
{
    public class CreateNotificationSettingsRequest
    {
        public string Email { get; set; } = string.Empty;
        public string TelegramBotToken { get; set; } = string.Empty;
        public string TelegramChatId { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public bool NotifyOnNewOrder { get; set; } = true;
        public bool NotifyOnStatusChange { get; set; } = true;
        public bool NotifyOnLowStock { get; set; } = true;
        public int LowStockThreshold { get; set; } = 5;
    }
}
