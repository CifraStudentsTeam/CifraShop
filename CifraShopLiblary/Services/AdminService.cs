using CifraShopLiblary.DataBase.Context;
using CifraShopLiblary.Models;
using CifraShopLiblary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShopLiblary.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationContext _context;

        public AdminService(ApplicationContext context)
            => _context = context;

        public async Task<List<Admin>> UploadingAdminData()
            => await _context.Admins.ToListAsync();

        public async Task<Admin> GetAdminByEmail(string email)
           => await _context.Admins.SingleOrDefaultAsync(x => x.EMail == email);

        public async Task<Admin> GetAdminById(int id)
           => await _context.Admins.SingleOrDefaultAsync(x => x.Id == id);


        public async Task<Admin> AuthenticationAdmin(string email, string password)
            => await _context.Admins.SingleOrDefaultAsync(x => x.EMail == email && x.Password == password);

        public async Task<Admin> RegisterAdmin(string name, string surName, string email, string password)
        {
            var admin = new Admin
            {
                Name = name,
                SurName = surName,
                EMail = email,
                Password = password
            };
            await _context.Admins.AddAsync(admin);
            await _context.SaveChangesAsync();
            return admin;
        }
        public async Task DeleteAdmin(Admin admin)
        {
            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();
        }

        public Task<Admin> UpdateAdmin(Admin admin)
        {
            throw new NotImplementedException();
        }
    }
}
