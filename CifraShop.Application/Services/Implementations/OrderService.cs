using CifraShop.Application.Services.Interfaces;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;

namespace CifraShop.Application.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IApiService _apiService;
        private const string _baseUri = "api/orders";

        //конструктор
        public OrderService(IApiService apiService)
            => _apiService = apiService;

        //создание заказа
        public async Task<Order> CreateOrderAsync(string studentLogin, List<OrderItem> items)
        {
            var request = new { StudentLogin = studentLogin, Items = items };
            return await _apiService.PostAsync<Order>(_baseUri, request);
        }
    
        // получение заказов студента
        public async Task<List<Order>> GetOrdersForStudentAsync(string studentLogin)
            => await _apiService.GetAsync<List<Order>>($"{_baseUri}/student/{studentLogin}");

        public async Task<Order> GetOrderByIdAsync(int id)
            => await _apiService.GetAsync<Order>($"{_baseUri}/ {id}");

        //обновление статуса заказа
        public async Task UpdateOrderStatusAsync(int orderId, StatusOrder newStatus)
        {
            var request = new {OrderId  = orderId, Status = newStatus};
            await _apiService.PutAsync($"{_baseUri}/{orderId}/status", request);
        }

        //отмена заказа
        public async Task CancelOrderAsync(int orderId)
            => await _apiService.DeleteAsync($"{_baseUri}/{orderId}");
    }
}
