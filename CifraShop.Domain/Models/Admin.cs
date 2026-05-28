using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CifraShop.Domain.Models
{
    public class Admin
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }
        public string? Name { get; set; }
        public string? SurName { get; set; }
        public string EMail { get; set; }
        public string? Password { get; set; }
    }
}
