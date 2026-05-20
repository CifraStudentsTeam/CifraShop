using CifraShopLiblary.Models;
using CifraShopTest.Base;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShopTest
{
    public class AdminSertviceTest : AdminServiceTestBase
    {
        [Fact]
        public async Task AdminRegister_ValidData_RetutnAdmin()
        {
            var name = "testname";
            var surName = "testsurname";
            var email = "testemail";
            var password = "testpassword";
            var result = await _service.RegisterAdmin(name, surName, email, password);
            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(surName, result.SurName);
            Assert.Equal(email, result.EMail);
            Assert.Equal(password, result.Password);
        }

        [Fact]
        public async Task AdminRegister_AddToDataBase_ReturnAdmin()
        {
            var name = "newadmin";
            await _service.RegisterAdmin(name, "newsurname", "newemail", "newpass");
            var admin = await _context.Admins.SingleOrDefaultAsync(x => x.Name == name);
            Assert.NotNull(admin);
        }


        [Fact]
        public async Task AdminAuthentication_ValiddCreditnals_ReturnAdmin()
        {
            var email = "authemail";
            var password = "authpassword";

            await _context.Admins.AddAsync(new Admin
            {
                Name = "authname",
                SurName = "authsurname",
                EMail = email,
                Password = password
            });

            await _context.SaveChangesAsync();
            var result = await _service.AuthenticationAdmin(email, password);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task AdminAuthentication_InvalidCredutnals_ReturnsNull()
        {
            var result = await _service.AuthenticationAdmin("invalidemail", "invalidpass");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAdminById_ExssistinId_RerurnsAdmin()
        {
            var admin = new Admin
            {
                Name = "name1",
                SurName = "surname1",
                EMail = "email1",
                Password = "pass1"
            };

            await _context.Admins.AddAsync(admin);
            await _context.SaveChangesAsync();
            var result = await _service.GetAdminById(admin.Id);
            Assert.NotNull(result);
            Assert.Equal(result.EMail, "email1");
        }

        [Fact]
        public async Task GetAdminById_NonExssistingId_ReturnsNull()
        {
            var result = await _service.GetAdminById(999);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAdminByEmail_EsssistingEmail_ReturnsAdmin()
        {
            var email = "emailEeail";

            await _context.Admins.AddAsync(new Admin
            {
                Name = "nameemail",
                SurName = "surnameemail",
                EMail = email,
                Password = "passwordemail"
            });
            
            await _context.SaveChangesAsync();
             var result = await _service.GetAdminByEmail(email);
            Assert.NotNull(result);
            Assert.Equal(result.EMail, email);
        }

        [Fact] 
        public async Task UpdatingAdminDara_ReturnAllUser()
        {
            await _context.AddRangeAsync
            (
                new Admin { Name = "namea", SurName = "surnamea", EMail = "emaila", Password = "passworda" },
                new Admin { Name = "nameb", SurName = "surnameb", EMail = "emailb", Password = "passwordb"},
                new Admin { Name = "namec", SurName = "surnamec", EMail = "emailc", Password = "passwordc" }
            );

            await _context.SaveChangesAsync();
            var result = await _service.UploadingAdminData();
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task DeleteAdmin_FromDatabase()
        {
            var admin = new Admin
            {
                Name = "deletename",
                SurName = "deletesurname",
                EMail = "deleteemail",
                Password = "deletepassword"
            };

            await _context.Admins.AddAsync(admin);
            await _context.SaveChangesAsync();
            await _service.DeleteAdmin(admin);
            var result = await _context.Admins.FindAsync(admin.Id);
            Assert.Null(result);
        }
    }
}
