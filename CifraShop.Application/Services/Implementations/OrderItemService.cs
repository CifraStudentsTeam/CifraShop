using CifraShop.Application.Services.Interfaces;
using CifraShop.Domain.Entities;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;

namespace CifraShop.Application.Services.Implementations
{
    public class OrderItemService : IOrderItemService
    {
        private readonly IOrderItemRepository _repository;
        private readonly IProductRepository _productRepository;

        public OrderItemService(IOrderItemRepository repository, IProductRepository productRepository)
        {
            _repository = repository;
            _productRepository = productRepository;
        }

        public Task<OrderItem> GetOrderItemById(int orderItemId)
            => _repository.GetOrderItemById(orderItemId);

        public Task<List<OrderItem>> GetOrderItemsByOrderId(int orderId)
            => _repository.GetOrderItemsByOrderId(orderId);

        public async Task<OrderItem> CreateOrderItem(int orderId, int productId, short quantity)
        {
            var product = await _productRepository.GetProductById(productId);

            var orderItem = new OrderItem
            {
                OrderId = orderId,
                ProductId = productId,
                Product = product,
                Price = product.Price,
                Quantity = quantity
            };

            await _repository.AddOrderItem(orderItem);
            return orderItem;
        }

        public Task UpdateOrderItem(OrderItem orderItemToUpdate)
            => _repository.UpdateOrderItem(orderItemToUpdate);

        public Task DeleteOrderItem(OrderItem orderItemToDelete)
            => _repository.DeleteOrderItem(orderItemToDelete);
    }
}
