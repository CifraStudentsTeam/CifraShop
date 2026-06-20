using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Responses.Common;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Domain.Repositories;

namespace CifraShop.Application.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IUserRepository _userRepository;
        private readonly IProductRepository _productRepository;

        public OrderService(
            IOrderRepository repository,
            IOrderItemRepository orderItemRepository,
            IUserRepository userRepository,
            IProductRepository productRepository)
        {
            _repository = repository;
            _orderItemRepository = orderItemRepository;
            _userRepository = userRepository;
            _productRepository = productRepository;
        }

        public Task<List<Order>> GetAllOrders()
            => _repository.GetAll();

        public async Task<PagedResponse<Order>> GetOrdersPaged(int page, int pageSize, string? search = null, StatusOrder? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            if (page < 0)
                throw new ArgumentException("Номер страницы не может быть отрицательным");
            if (pageSize <= 0)
                throw new ArgumentException("Размер страницы должен быть больше 0");

            var (items, total) = await _repository.GetAllPaged(page, pageSize, search, status, dateFrom, dateTo);
            return new PagedResponse<Order> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }

        public Task<Order?> GetOrderById(int id)
            => _repository.GetOrderById(id);

        public Task<List<Order>> GetOrdersByCustomerEmail(string email)
            => _repository.GetOrdersByCustomerEmail(email);

        public Task<List<Order>> GetOrdersByStatus(StatusOrder status)
            => _repository.GetOrdersByStatus(status);

        public Task<List<Order>> GetOrdersBySum(short sum)
            => _repository.GetOrdersBySum(sum);

        public Task<List<Order>> GetOrdersByDateRange(DateTime from, DateTime to)
            => _repository.GetOrdersByDateRange(from, to);

        public async Task<Order> CreateOrder(string customerEmail, List<(int ProductId, short Quantity)> items)
        {
            if (string.IsNullOrWhiteSpace(customerEmail))
                throw new ArgumentException("Email пользователя обязателен");

            if (items == null || items.Count == 0)
                throw new ArgumentException("Заказ должен содержать хотя бы один товар");

            var duplicateIds = items.GroupBy(x => x.ProductId).Where(g => g.Count() > 1).Select(g => g.Key);
            if (duplicateIds.Any())
                throw new ArgumentException($"Дублирующиеся товары в заказе: id={string.Join(", ", duplicateIds)}");

            var user = await _userRepository.GetUserByEmail(customerEmail);
            if (user == null)
                throw new KeyNotFoundException($"Пользователь с email {customerEmail} не найден");

            var orderItems = new List<OrderItem>();
            var stockUpdates = new List<(int ProductId, short Quantity)>();
            int totalSum = 0;

            foreach (var (productId, quantity) in items)
            {
                if (quantity <= 0)
                    throw new ArgumentException($"Количество товара id={productId} должно быть больше 0");

                var product = await _productRepository.GetProductById(productId);
                if (product == null)
                    throw new KeyNotFoundException($"Товар с id={productId} не найден");

                if (product.Quantity < quantity)
                    throw new InvalidOperationException(
                        $"Недостаточно товара \"{product.Name}\" (доступно {product.Quantity}, запрошено {quantity})");

                orderItems.Add(new OrderItem
                {
                    ProductId = productId,
                    Price = product.Price,
                    Quantity = quantity
                });

                totalSum += product.Price * quantity;
                stockUpdates.Add((productId, quantity));
            }

            if (totalSum > short.MaxValue)
                throw new OverflowException($"Сумма заказа ({totalSum}) превышает максимально допустимое значение");

            var order = new Order
            {
                Status = StatusOrder.Pending,
                Sum = (short)totalSum,
                DateOfPurchase = DateTime.UtcNow,
                CustomerId = user.Id
            };

            return await _repository.CreateOrderInTransaction(order, orderItems, stockUpdates);
        }

        public async Task UpdateOrder(Order orderToUpdate)
        {
            if (orderToUpdate == null)
                throw new ArgumentNullException(nameof(orderToUpdate));

            var existing = await _repository.GetOrderById(orderToUpdate.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Заказ с id {orderToUpdate.Id} не найден");

            await _repository.UpdateOrder(orderToUpdate);
        }

        public async Task UpdateStatusRange(List<int> ids, StatusOrder newStatus)
        {
            if (ids == null || ids.Count == 0)
                throw new ArgumentException("Список id не может быть пустым");

            await _repository.UpdateStatusRange(ids, newStatus);
        }

        public async Task DeleteOrder(Order orderToDelete)
        {
            var items = await _orderItemRepository.GetOrderItemsByOrderId(orderToDelete.Id);

            foreach (var item in items)
            {
                var product = await _productRepository.GetProductById(item.ProductId);
                if (product != null)
                {
                    product.Quantity += item.Quantity;
                    product.Status = product.Quantity > 0
                        ? StatusProduct.InStock
                        : StatusProduct.OutOfStock;
                    await _productRepository.UpdateProduct(product);
                }
            }

            foreach (var item in items)
                await _orderItemRepository.DeleteOrderItem(item);

            await _repository.DeleteOrder(orderToDelete);
        }
    }
}
