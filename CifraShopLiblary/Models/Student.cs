using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShopLiblary.Models
{
    public class Student
    {
        public uint Id { get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }
        public DateTime DateOfBirth { get; set; }
        public uint Balance { get; set; }
    }
}
