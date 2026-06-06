using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Infrastructure.Data.Repositories.Interfaces;

public interface IOrderRepository
{
    public Task<List<Order>> UploadingOrderData();
    public Task<Order> CreateOrder(StatusOrder order, uint sum, string login);
    public Task<Order> GetOrderById(int id);
    public Task<List<Order>> GetOrdersByLogin(string login);
    public Task<List<Order>> GetOrdersBySum(uint sum);
    public Task<List<Order>> GetOrdersByStatus(StatusOrder order);
    public Task<Order> ChangeOrderStatus(Order orderToChange, StatusOrder statusToChange);
    public Task UpdateOrder(Order orderToUpdate);
    public Task DeleteOrder(Order orderToDelete);
}
