using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CifraShop.Components.Models
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
        [Column("Password")]
        public string Password { get; set; }
    }
}
