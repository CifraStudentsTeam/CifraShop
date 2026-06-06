using CifraShop.Domain.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email {  get; set; }
        public string Password { get; set; }
        public short? Balance { get; set; }
        public UserRole Role { get; set; }
        //public ICollection Orders {  get; set; } = new List<Order>();
        public List<Order> Orders { get; set; } = new List<Order>(); 
    }
}
