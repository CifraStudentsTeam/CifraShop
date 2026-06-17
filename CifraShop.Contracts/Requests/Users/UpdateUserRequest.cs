using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Contracts.Requests.Users
{
    public class UpdateUserRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public short? Balance { get; set; }
    }
}
