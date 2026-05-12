using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CifraShopLiblary.Models
{
    public class Admin
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }
        public string EMail { get; set; }
        public string Password { get; set; }
    }
}
