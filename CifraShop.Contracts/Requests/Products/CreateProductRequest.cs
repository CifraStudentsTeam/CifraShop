using System.ComponentModel.DataAnnotations;

namespace CifraShop.Contracts.Requests.Products
{
    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Название товара обязательно")]
        [MaxLength(30, ErrorMessage = "Название не может превышать 30 символов")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Описание товара обязательно")]
        [MaxLength(100, ErrorMessage = "Описание не может превышать 100 символов")]
        public string Description { get; set; } = string.Empty;

        [Range(1, short.MaxValue, ErrorMessage = "Цена должна быть больше 0")]
        public short Price { get; set; }

        [Range(0, short.MaxValue, ErrorMessage = "Количество не может быть отрицательным")]
        public short Quantity { get; set; }
    }
}
