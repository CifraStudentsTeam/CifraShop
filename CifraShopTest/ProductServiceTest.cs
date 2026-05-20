using CifraShopLiblary.Data.DataForModels;
using CifraShopLiblary.Models;
using CifraShopTest.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShopTest
{
    public class ProductServiceTest : ProductServiceTestBase
    {
        [Fact]
        public async Task CreateProduct_ValidData_ReturnStudent()
        {
            var name = "createproduct";
            var description = "createdescription";
            uint price = 100;
            uint quntity = 1;
            var status = StatusProduct.InStock;
            var result = await _service.CreateProduct(name, description, price, quntity, status);
            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(description, result.Description);
            Assert.Equal(price, result.Price);
            Assert.Equal(quntity, result.Quantity);
            Assert.Equal(status, result.Status);
        }

        [Fact]
        public async Task CreateProduct_AddToDatabase_ReturnStudent()
        {
            var name = "createproduct";
            await _service.CreateProduct(name, "creaateddescription", 100, 1, StatusProduct.InStock);
            var result = await _context.Products.SingleOrDefaultAsync(x  => x.Name == name);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetProductsByName_ExisstingName_ReturnProducts()
        {
            var name = "getname";

            await _context.AddRangeAsync(
                new Product { Name = name, Description = "getnamedescriprion", Price = 100, Quantity = 1, Status = StatusProduct.InStock },
                new Product { Name = name, Description = "getnamedescriprion", Price = 100, Quantity = 1, Status = StatusProduct.InStock },
                new Product { Name = name, Description = "noonamedescriprion", Price = 100, Quantity = 1, Status = StatusProduct.InStock }
            );

            await _context.SaveChangesAsync();
            var result = await _service.GetProductsByName(name);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, x => Assert.Equal(name, x.Name));
        }

        [Fact]
        public async Task GetProductsByPrice_ExisstingPrice_ReturnProducts()
        {
            uint price = 101;

            await _context.AddRangeAsync(
                new Product { Name = "pricename", Description = "pricedescription", Price = price, Quantity = 1, Status = StatusProduct.InStock },
                new Product { Name = "pricename", Description = "pricedescription", Price = price, Quantity = 1, Status = StatusProduct.InStock },
                new Product { Name = "namename", Description = "pricedescription", Price = price, Quantity = 1, Status = StatusProduct.InStock}
            );
            await _context.SaveChangesAsync();
            var result = await _service.GetProductsByPrice(price);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, x => Assert.Equal(price, x.Price));
        }

        [Fact]
        public async Task GetProductsByQuantity_ExisstingQuantity_ReturnProducts()
        {
            uint quantity = 2;

            await _context.AddRangeAsync(
                new Product { Name = "quantityname", Description = "quantitydescription", Price = 100, Quantity = quantity, Status = StatusProduct.InStock },
                new Product { Name = "quantityname", Description = "quantitydescription", Price = 100, Quantity = quantity, Status = StatusProduct.InStock },
                new Product { Name = "quantityname", Description = "quantitydescription", Price = 100, Quantity = 1, Status = StatusProduct.InStock }
            );
            await _context.SaveChangesAsync();
            var result = await  _service.GetProductsByQuntity(quantity);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, x => Assert.Equal(quantity, x.Quantity));
        }

        [Fact]
        public async Task GetProductsByStatus_ExisstingPrice_ReturnProducts()
        {
            var status = StatusProduct.OnSaleSoon;

            await _context.AddRangeAsync(
                new Product { Name = "statusname", Description = "statusdescription", Price = 100, Quantity = 1,}
            );
        }
    }
}
