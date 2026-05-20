using CifraShopLiblary.DataBase.Context;
using CifraShopLiblary.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShopTest.Base
{
    public class ProductServiceTestBase : IDisposable
    {         
        protected readonly ApplicationContext _context;
        protected readonly ProductService _service;

        public ProductServiceTestBase()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationContext(options);
            _service = new ProductService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
        
    }
}
