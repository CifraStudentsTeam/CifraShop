using System.ComponentModel.DataAnnotations;

namespace CifraShop.Contracts.Requests.OrderItem
{
    public class CreateOrderItemRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Id заказа должен быть больше 0")]
        public int OrderId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Id товара должен быть больше 0")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
        public int Quantity { get; set; }
    }
}
