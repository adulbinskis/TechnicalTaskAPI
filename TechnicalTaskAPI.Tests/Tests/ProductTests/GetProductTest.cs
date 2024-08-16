using System;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechnicalTaskAPI.Application.Common;
using TechnicalTaskAPI.Application.Products.Models;
using TechnicalTaskAPI.Application.Products.Querirs;
using TechnicalTaskAPI.ORM.Entities;
using TechnicalTaskAPI.ORM.Services;
using TechnicalTaskAPI.Tests.Constants;
using TechnicalTaskAPI.Tests.Fixtures;
using Xunit;

namespace TechnicalTaskAPI.Tests.Tests.Products
{
    [Collection("Database collection")]
    public class GetProductTests : IClassFixture<DatabaseFixture>
    {
        private readonly IMediator _mediator;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public GetProductTests(DatabaseFixture fixture)
        {
            _mediator = fixture.ServiceProvider.GetRequiredService<IMediator>();
            _context = fixture.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _mapper = fixture.ServiceProvider.GetRequiredService<IMapper>();
            _configuration = fixture.Configuration;
        }

        private async Task SeedProduct()
        {
            var existingProduct = await _context.Products.FirstOrDefaultAsync(x => x.Id == new Guid(TestProductConstants.Default_Product_Id));
            if (existingProduct == null)
            {
                var product = new Product
                {
                    Id = new Guid(TestProductConstants.Default_Product_Id),
                    Name = TestProductConstants.Default_Name,
                    Quantity = TestProductConstants.Default_Quantity,
                    PricePerUnit = TestProductConstants.Default_PricePerUnit
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("Product already exists, skipping seeding.");
            }
        }

        [Fact]
        public async Task GetProduct_Success()
        {
            // Arrange
            await SeedProduct();
            var productId = new Guid(TestProductConstants.Default_Product_Id);
            var query = new GetProduct.Query { ProductId = productId };

            // Act
            var result = await _mediator.Send(query);

            var vatRate = _configuration.GetSection("SiteSettings").Get<SiteSettings>().VatRate;

            var vat = (TestProductConstants.Default_Quantity * TestProductConstants.Default_PricePerUnit) * (1 + vatRate);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.Id);
            Assert.Equal(TestProductConstants.Default_Name, result.Name);
            Assert.Equal(TestProductConstants.Default_Quantity, result.Quantity);
            Assert.Equal(TestProductConstants.Default_PricePerUnit, result.PricePerUnit);
            Assert.Equal(vat, result.PriceWithVAT);
        }

        [Fact]
        public async Task GetProduct_ProductNotFound()
        {
            // Arrange
            var query = new GetProduct.Query { ProductId = Guid.NewGuid() };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _mediator.Send(query));
            Assert.Equal("Product not found", exception.Message);
        }

        [Fact]
        public async Task GetProduct_InvalidProductIdFormat()
        {
            // Arrange
            var query = new GetProduct.Query { ProductId = Guid.Empty };
            var validator = new GetProduct.Query.Validator();

            // Act
            var validationResult = await validator.ValidateAsync(query);

            // Assert
            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.PropertyName == nameof(query.ProductId));
        }
    }
}
