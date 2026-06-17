using CifraShop.Contracts.Responses.User;
using CifraShop.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CifraShop.Contracts.Mappings
{
    public static class UserMapping
    {
        public static UserResponse ToResponse(this User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Password = user.Password,
                Balance = user.Balance,
                Role = user.Role
            };
        }
    }
}
