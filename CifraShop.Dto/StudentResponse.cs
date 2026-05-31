using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Dto
{
    public class StudentResponse
    {
        public uint Id { get; set; }
        public string LoginName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public uint Balance { get; set; }
    }
}
