using CifraShop.Application.Services.Interfaces;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Domain.Repositories;

namespace CifraShop.Application.Services.Implementations
{
    public class OrderItemService : IOrderItemService
    {
        private readonly IOrderItemRepository _repository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;

        public OrderItemService(
            IOrderItemRepository repository,
            IProductRepository productRepository,
            IOrderRepository orderRepository)
        {
            _repository = repository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
        }

        public Task<OrderItem?> GetOrderItemById(int orderItemId)
            => _repository.GetOrderItemById(orderItemId);

        public Task<List<OrderItem>> GetOrderItemsByOrderId(int orderId)
            => _repository.GetOrderItemsByOrderId(orderId);

        public async Task<OrderItem> CreateOrderItem(int orderId, int productId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Количество должно быть больше 0");

            var order = await _orderRepository.GetOrderById(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Заказ с id={orderId} не найден");

            var product = await _productRepository.GetProductById(productId);
            if (product == null)
                throw new KeyNotFoundException($"Товар с id={productId} не найден");

            if (product.Quantity < quantity)
                throw new InvalidOperationException(
                    $"Недостаточно товара \"{product.Name}\" (доступно {product.Quantity}, запрошено {quantity})");

            var orderItem = new OrderItem
            {
                OrderId = orderId,
                ProductId = productId,
                Price = product.Price,
                Quantity = quantity
            };

            product.Quantity -= quantity;
            product.Status = product.Quantity == 0
                ? StatusProduct.OutOfStock
                : StatusProduct.InStock;

            await _repository.AddOrderItem(orderItem);
            await _productRepository.UpdateProduct(product);

            return orderItem;
        }

        public Task UpdateOrderItem(OrderItem orderItemToUpdate)
        {
            if (orderItemToUpdate == null)
                throw new ArgumentNullException(nameof(orderItemToUpdate));
            return _repository.UpdateOrderItem(orderItemToUpdate);
        }

        public async Task DeleteOrderItem(OrderItem orderItemToDelete)
        {
            var product = await _productRepository.GetProductById(orderItemToDelete.ProductId);
            if (product != null)
            {
                product.Quantity += orderItemToDelete.Quantity;
                product.Status = product.Quantity > 0
                    ? StatusProduct.InStock
                    : StatusProduct.OutOfStock;
                await _productRepository.UpdateProduct(product);
            }

            await _repository.DeleteOrderItem(orderItemToDelete);
        }
    }
}
