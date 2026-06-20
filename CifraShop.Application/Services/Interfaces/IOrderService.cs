using CifraShop.Contracts.Responses.Common;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;

namespace CifraShop.Application.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllOrders();
        Task<PagedResponse<Order>> GetOrdersPaged(int page, int pageSize, string? search = null, StatusOrder? status = null, DateTime? dateFrom = null, DateTime? dateTo = null);
        Task<Order?> GetOrderById(int id);
        Task<List<Order>> GetOrdersByStatus(StatusOrder status);
        Task<List<Order>> GetOrdersBySum(int sum);
        Task<List<Order>> GetOrdersByCustomerEmail(string email);
        Task<List<Order>> GetOrdersByDateRange(DateTime from, DateTime to);
        Task<Order> CreateOrder(string customerEmail, List<(int ProductId, int Quantity)> items);
        Task UpdateOrder(Order orderToUpdate);
        Task UpdateStatusRange(List<int> ids, StatusOrder newStatus);
        Task DeleteOrder(Order orderToDelete);
    }
}
