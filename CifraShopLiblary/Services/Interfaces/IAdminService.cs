using CifraShopLiblary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShopLiblary.Services.Interfaces
{
    public interface IAdminService
    {
        public Task<List<Admin>> UploadingAdminData();
        public Task<Admin> RegisterAdmin(string name, string surName, string email, string password);
        public Task<Admin> AuthenticationAdmin(string email, string password);
        public Task<Admin> GetAdminByEmail(string email);
        public Task<Admin> GetAdminById(uint id);
        public Task DeleteAdmin(Admin admin);
    }
}
