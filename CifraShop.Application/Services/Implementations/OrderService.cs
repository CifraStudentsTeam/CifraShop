using CifraShop.Application.Services.Interfaces;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;

namespace CifraShop.Application.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;

        public OrderService(IOrderRepository repository)
            => _repository = repository;

        public Task<List<Order>> GetAllOrders()
            => _repository.GetAll();

        public Task<Order> GetOrderById(int id)
            => _repository.GetOrderById(id);

        public Task<List<Order>> GetOrdersByCustomerLogin(string customerLogin)
            => _repository.GetOrdersByLogin(customerLogin);

        public Task<List<Order>> GetOrdersByStatus(StatusOrder status)
            => _repository.GetOrdersByStatus(status);

        public Task<List<Order>> GetOrdersBySum(short sum)
            => _repository.GetOrdersBySum(sum);

        public async Task<Order> CreateOrder(short sum, int customerId, string customerLogin, List<OrderItem> orderItems)
        {
            var order = new Order
            {
                Status = StatusOrder.Pending,
                Sum = sum,
                DateOfPurchase = DateTime.Now,
                CustomerLogin = customerLogin,
                CustomerId = customerId,
                OrderItems = orderItems
            };

            await _repository.AddOrder(order);
            return order;
        }

        public Task UpdateOrder(Order orderToUpdate)
            => _repository.UpdateOrder(orderToUpdate);

        public Task DeleteOrder(Order orderToDelete)
            => _repository.DeleteOrder(orderToDelete);
    }
}
