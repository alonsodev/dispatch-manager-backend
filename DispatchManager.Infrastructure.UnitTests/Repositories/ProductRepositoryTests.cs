using DispatchManager.Domain.Entities;
using DispatchManager.Infrastructure.Data;
using DispatchManager.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace DispatchManager.Infrastructure.UnitTests.Repositories;

[TestFixture]
public sealed class ProductRepositoryTests
{
    private DispatchManagerDbContext _context;
    private ProductRepository _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<DispatchManagerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DispatchManagerDbContext(options);
        _repository = new ProductRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task SearchByNameAsync_WithPartialName_ShouldReturnMatchingProducts()
    {
        // Arrange
        var product1 = Product.Create("Laptop Dell", "Dell Inspiron", 2500m, "units");
        var product2 = Product.Create("Laptop HP", "HP Pavilion", 2200m, "units");
        var product3 = Product.Create("Mouse", "Wireless Mouse", 50m, "units");

        await _repository.AddAsync(product1);
        await _repository.AddAsync(product2);
        await _repository.AddAsync(product3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchByNameAsync("Laptop");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.Name.Contains("Laptop"));
        result.Should().BeInAscendingOrder(p => p.Name);
    }

    [Test]
    public async Task GetProductsByPriceRangeAsync_ShouldReturnProductsInRange()
    {
        // Arrange
        var product1 = Product.Create("Expensive Product", "Very expensive", 5000m, "units");
        var product2 = Product.Create("Medium Product", "Medium price", 1500m, "units");
        var product3 = Product.Create("Cheap Product", "Very cheap", 100m, "units");

        await _repository.AddAsync(product1);
        await _repository.AddAsync(product2);
        await _repository.AddAsync(product3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetProductsByPriceRangeAsync(500m, 2000m);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Medium Product");
        result.Should().BeInAscendingOrder(p => p.UnitPrice);
    }

    [Test]
    public async Task GetAverageProductPriceAsync_ShouldReturnCorrectAverage()
    {
        // Arrange
        var product1 = Product.Create("Product 1", "Description", 100m, "units");
        var product2 = Product.Create("Product 2", "Description", 200m, "units");
        var product3 = Product.Create("Product 3", "Description", 300m, "units");

        await _repository.AddAsync(product1);
        await _repository.AddAsync(product2);
        await _repository.AddAsync(product3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAverageProductPriceAsync();

        // Assert
        result.Should().Be(200m);
    }

    [Test]
    public async Task GetPriceRangeAsync_ShouldReturnMinAndMaxPrices()
    {
        // Arrange
        var product1 = Product.Create("Cheap Product", "Description", 50m, "units");
        var product2 = Product.Create("Medium Product", "Description", 500m, "units");
        var product3 = Product.Create("Expensive Product", "Description", 2000m, "units");

        await _repository.AddAsync(product1);
        await _repository.AddAsync(product2);
        await _repository.AddAsync(product3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPriceRangeAsync();

        // Assert
        result.Min.Should().Be(50m);
        result.Max.Should().Be(2000m);
    }

    [Test]
    public async Task GetProductListAsync_ShouldReturnOrderedProductsList()
    {
        // Arrange
        var product1 = Product.Create("Zebra Product", "Description", 100m, "units");
        var product2 = Product.Create("Alpha Product", "Description", 200m, "units");
        var product3 = Product.Create("Beta Product", "Description", 150m, "units");

        await _repository.AddAsync(product1);
        await _repository.AddAsync(product2);
        await _repository.AddAsync(product3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetProductListAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeInAscendingOrder(x => x.Name);
        result.First().Name.Should().Be("Alpha Product");
        result.Last().Name.Should().Be("Zebra Product");
    }
}