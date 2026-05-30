using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Dto
{
    public class RegisterAdminRequest
    {
        public string Name { get; set; }
        public string SurName {  get; set; }
        public string EMail { get; set; }
        public string Password { get; set; }
    }
}
