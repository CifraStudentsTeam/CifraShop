using CifraShopLiblary.DataBase.Context;
using CifraShopLiblary.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShopTest.Base
{
    public abstract class StudentServiceTestBase : IDisposable
    {
        protected readonly ApplicationContext _context;
        protected readonly StudentService _sut;

        protected StudentServiceTestBase()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationContext(options);
            _sut = new StudentService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
