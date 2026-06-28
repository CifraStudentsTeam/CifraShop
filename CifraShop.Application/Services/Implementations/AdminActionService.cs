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

        public Task<List<AdminAction>> GetLastActions(int count, string? branch = null)
        {
            if (count <= 0)
                throw new ArgumentException("Количество действий должно быть больше 0");
            return _repository.GetLastActions(count, branch);
        }

        public async Task AddAction(string type, string details, string branch)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Тип действия обязателен");
            if (string.IsNullOrWhiteSpace(details))
                throw new ArgumentException("Детали действия обязательны");

            var action = new AdminAction
            {
                ActionType = type,
                Details = details,
                Branch = branch ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAction(action);
        }

        public Task TrimOldActions(int keepCount)
        {
            return _repository.TrimOldActions(keepCount);
        }
    }
}
