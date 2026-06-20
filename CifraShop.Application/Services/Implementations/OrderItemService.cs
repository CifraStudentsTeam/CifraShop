using CifraShop.Application.Services.Interfaces;
using CifraShop.Domain.Entities;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Application.Services.Implementations
{
    public class OrderItemService : IOrderItemService
    {
        public IOrderItemRepository _repository;

        //Конструктор
        public OrderItemService(IOrderItemRepository repository)
            => _repository = repository;

        //Получение составляющей заказа по id 
        public Task<OrderItem> GetOrderItemById(int orderItemId)
            => _repository.GetOrderItemById(orderItemId);

        //Получениие состовляющей заказа по id заказа
        public Task<List<OrderItem>> GetOrderItemsByOrderId(int orderId)
            => _repository.GetOrderItemsByOrderId(orderId);

        //Создание состовляющей заказа
        public async Task CreateOrderItem(Order order, Product product, short quantity)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                OrderInOrder = order,
                ProductId = product.Id,
                ProductInOrder = product,
                Price = product.Price,
            };

            await _repository.AddOrderItem(orderItem);
        }

        //Обновление состовляющей заказа
        public Task UpdateOrderItem(OrderItem orderItemToUpdate)
            => _repository.UpdateOrderItem(orderItemToUpdate);

        //Удаление состовляющей заказа
        public Task DeleteOrderItem(OrderItem orderItemToDelete)
            => _repository.DeleteOrderItem(orderItemToDelete);
    }
}
