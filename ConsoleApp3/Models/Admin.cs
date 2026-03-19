using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3.Models
{
    public class Admin
    {
        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }
        [Column("Name")]
        public string Name { get; set; }
        [Column("SurName")]
        public string SurName { get; set; }
        [Column("EMail")]
        public string EMail { get; set; }
        [Column ("Password")]
        public string Password { get; set; }
        
    }
}
