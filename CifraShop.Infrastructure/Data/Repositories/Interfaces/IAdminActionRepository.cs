using CifraShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Infrastructure.Data.Repositories.Interfaces
{
    public interface IAdminActionRepository
    {
        Task<List<AdminAction>> GetLastActions(int count);
        Task AddAction(AdminAction action);
    }
}
