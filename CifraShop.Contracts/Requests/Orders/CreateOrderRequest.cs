using CifraShop.Contracts.Requests.OrderItem;
using System.ComponentModel.DataAnnotations;

namespace CifraShop.Contracts.Requests.Orders
{
    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "Email покупателя обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Список товаров обязателен")]
        [MinLength(1, ErrorMessage = "Заказ должен содержать хотя бы один товар")]
        public List<OrderItemRequest> Items { get; set; } = new();
    }
}
