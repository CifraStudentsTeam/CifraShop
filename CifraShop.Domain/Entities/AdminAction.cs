using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Domain.Entities
{
    public class AdminAction
    {
        public int Id { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
