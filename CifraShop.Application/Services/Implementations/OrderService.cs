using CifraShop.Application.Services.Interfaces;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Application.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;

        //Конструктор
        public OrderService(IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, IProductRepository productRepository, IUserRepository userRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
        }
        public async Task<Order> CreateCardAsync(string customerLogin)
            => await _orderRepository.CreateOrder(StatusOrder.Pending, 0, customerLogin);

        public async Task<Order?> GetOrderById(int id)
            => await _orderRepository.GetOrderById(id);

        public async Task<List<Order>> GetAllOrder()
            => await _orderRepository.UploadingOrderData();

        public async Task<List<Order>> GetOrdersByLogin(string login)
            => await _orderRepository.GetOrdersByLogin(login);

        public async Task<List<Order>> GetOrderByStatus(StatusOrder status)
            => await _orderRepository.GetOrdersByStatus(status);

        public async Task<List<Order>> GetOrdersBySum(short sum)
            => await _orderRepository.GetOrdersBySum(sum);
    
        public Task ChangeCustomerLogin(Order order, string newLogin)
        {
            throw new NotImplementedException();
        }

        public Task ChangeOrderStatus(Order order, StatusOrder newStatus)
        {
            throw new NotImplementedException();
        }


        public Task DeleteOrder(Order order)
        {
            throw new NotImplementedException();
        }




        public Task<Order> RemoveOrderItem(int orderItemId)
        {
            throw new NotImplementedException();
        }

        public Task<Order> AddProductToOrder(int orderId, int productId, short quantity)
        {
            throw new NotImplementedException();
        }
    }
}
