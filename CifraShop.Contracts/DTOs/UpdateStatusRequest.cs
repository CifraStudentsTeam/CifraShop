using System;
using System.Collections.Generic;
using System.Text;
using CifraShop.Domain.Enums;

namespace CifraShop.Contracts.DTOs
{
    public class UpdateStatusRequest
    {
        public StatusProduct Status { get; set; }
    }
}
