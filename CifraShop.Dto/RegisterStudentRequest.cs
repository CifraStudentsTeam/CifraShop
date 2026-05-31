using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Dto
{
    public class RegisterStudentRequest
    {
        public string LoginName { get; set; }
        public string Password { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
