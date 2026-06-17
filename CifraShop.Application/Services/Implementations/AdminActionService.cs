using CifraShop.Application.Services.Interfaces;
using CifraShop.Domain.Entities;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;

namespace CifraShop.Application.Services.Implementations
{
    public class AdminActionService : IAdminActionService
    {
        private readonly IAdminActionRepository _repository;

        public AdminActionService(IAdminActionRepository repository)
            => _repository = repository;

        public Task<List<AdminAction>> GetLastActions(int count)
            => _repository.GetLastActions(count);

        public async Task AddAction(string type, string details)
        {
            var action = new AdminAction
            {
                ActionType = type,
                Details = details,
                CreatedAt = DateTime.Now
            };
            await _repository.AddAction(action);
        }
    }
}
