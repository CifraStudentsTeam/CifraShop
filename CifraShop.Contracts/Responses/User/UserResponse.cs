using System;
using System.Collections.Generic;
using System.Text;
using CifraShop.Domain.Enums;

namespace CifraShop.Contracts.Responses.User
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public short? Balance { get; set; }
        public UserRole Role { get; set; }
    }
}
