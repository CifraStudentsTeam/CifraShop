using CifraShop.Contracts.Requests.OrderItem;
using System.ComponentModel.DataAnnotations;

namespace CifraShop.Contracts.Requests.Orders
{
    public class CreateOrderRequest
    {
        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public string Branch { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public List<OrderItemRequest> Items { get; set; } = new();
    }
}
