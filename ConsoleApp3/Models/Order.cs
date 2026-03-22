using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3.Models
{
    public class Order
    {
        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }
        //[Column("Number")]
        //public uint Number { get; set; }
        [Column("Status")]
        public StatusOrder Status { get; set; }
        [Column("Sum")]
        public uint Sum { get; set; }
        
        [Column("DateOfPurchase")]
        public DateTime DateOfPurchase { get; set; } = DateTime.Now;

        [Column ("CustomerLogin")]
        public string CustomerLogin{ get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        //добваить колекцию 
    }

    public enum StatusOrder
    {
        AwaitingPayment,
        PaidFor,
        ManufacturedBy,
        Completed
    }
}
