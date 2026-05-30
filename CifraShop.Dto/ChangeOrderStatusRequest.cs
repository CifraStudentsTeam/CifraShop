using CifraShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Dto
{
    public class ChangeOrderStatusRequest
    {
        public StatusOrder NewStatus {  get; set; }
    }
}
