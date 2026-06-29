using CifraShop.Contracts.Requests.OrderItem;
using System.ComponentModel.DataAnnotations;

namespace CifraShop.Contracts.Requests.Orders
{
    public class CreateOrderRequest
    {
        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        public string? Branch { get; set; }

        [Required]
        [MinLength(1)]
        public List<OrderItemRequest> Items { get; set; } = new();
    }
}
