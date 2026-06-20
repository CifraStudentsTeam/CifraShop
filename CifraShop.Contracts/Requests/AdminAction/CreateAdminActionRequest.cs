using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Contracts.Requests.AdminAction
{
    public class CreateAdminActionRequest
    {
        public string ActionType { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
    }
}
