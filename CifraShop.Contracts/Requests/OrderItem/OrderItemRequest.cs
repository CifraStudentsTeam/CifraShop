using System.ComponentModel.DataAnnotations;

namespace CifraShop.Contracts.Requests.OrderItem
{
    public class OrderItemRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Id товара должен быть больше 0")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
        public int Quantity { get; set; }
    }
}
