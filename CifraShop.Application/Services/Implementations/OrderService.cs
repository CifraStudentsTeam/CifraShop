using CifraShop.Application.Services.Interfaces;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;

namespace CifraShop.Application.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IApiService _apiService;
        private const string _baseUri = "api/orders";

        public OrderService(IApiService apiService)
            => _apiService = apiService;

        #region Создание заказа
        public async Task<Order> CreateOrderAsync(string studentLogin, List<OrderItem> items)
        {
            var request = new { StudentLogin = studentLogin, Items = items };
            return await _apiService.PostAsync<Order>(_baseUri, request);
        }
        #endregion

        #region Чтение данных
        public Task<List<Order>> GetOrdersForStudentAsync(string studentLogin)
        {
            throw new NotImplementedException();
        }

        public Task<Order> GetOrderByIdAsync(uint id)
        {
            throw new NotImplementedException();
        }
        #endregion

        public Task UpdateOrderStatusAsync(uint orderId, StatusOrder newStatus)
        {
            throw new NotImplementedException();
        }

        public Task CancelOrderAsync(uint orderId)
        {
            throw new NotImplementedException();
        }
    }
}
