using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Contracts.Responses.AdminAction
{
    public class AdminActionResponse
    {
        public int Id { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
