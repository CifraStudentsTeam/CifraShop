using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CifraShop.Components.Models
{
    public class Student
    {
        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }
        [Column("LoginName")]
        public string LoginName { get; set; }
        [Column("Password")]
        public string Password { get; set; }
        [Column("DateOfBirth")]
        public DateTime DateOfBirth { get; set; }
        [Column("Balance")]
        public uint Balance { get; set; }
    }
}
