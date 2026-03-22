using CifraShop.Components.Models;

namespace CifraShop.Components.Services
{
    public interface IAdminService
    {
        public Task<List<Admin>> UploadingAdminData();
        public Task<Admin> RegisterAdmin(string name, string surName, string email, string password);
        public Task<Admin> AuthenticationAdmin(string email, string password);
        public Task<Admin> GetAdminByEmail(string email);
        public Task<Admin> GetAdminById(int id);
        public Task DeleteAdmin(Admin admin);
    }
}
