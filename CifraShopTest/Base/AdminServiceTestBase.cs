using CifraShopLiblary.DataBase.Context;
using CifraShopLiblary.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShopTest.Base
{
    public  class AdminServiceTestBase : IDisposable
    {
        protected readonly ApplicationContext _context;
        protected readonly AdminService _service;
        public AdminServiceTestBase(ApplicationContext _context, AdminService _service) {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationContext(options);
            _service = new AdminService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
