using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3.Models
{
    public class OrderItem
    {
        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }
        [Column ("OrderId")] 
        public uint OrderId { get; set; }
        [Column ("ProductName")]
        public uint ProductId { get; set; }
        [Column ("Quantity")]
        public uint Quantity { get; set; }
        [Column("Price")]
        public uint Price { get; set; }
    }
}
