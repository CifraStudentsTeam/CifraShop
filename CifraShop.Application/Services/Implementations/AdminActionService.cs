using CifraShop.Application.Services.Interfaces;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Repositories;

namespace CifraShop.Application.Services.Implementations
{
    public class AdminActionService : IAdminActionService
    {
        private readonly IAdminActionRepository _repository;

        public AdminActionService(IAdminActionRepository repository)
            => _repository = repository;

        public Task<List<AdminAction>> GetLastActions(int count)
        {
            if (count <= 0)
                throw new ArgumentException("Количество действий должно быть больше 0");
            return _repository.GetLastActions(count);
        }

        public async Task AddAction(string type, string details)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Тип действия обязателен");
            if (string.IsNullOrWhiteSpace(details))
                throw new ArgumentException("Детали действия обязательны");

            var action = new AdminAction
            {
                ActionType = type,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAction(action);
        }
    }
}
