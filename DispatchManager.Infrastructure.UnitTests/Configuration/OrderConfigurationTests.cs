using DispatchManager.Domain.Entities;
using DispatchManager.Domain.ValueObjects;
using DispatchManager.Domain.Enums;
using DispatchManager.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace DispatchManager.Infrastructure.UnitTests.Configuration;

[TestFixture]
public sealed class OrderConfigurationTests
{
    private DispatchManagerDbContext _context;
    private Customer _testCustomer;
    private Product _testProduct;

    [SetUp]
    public async Task SetUp()
    {
        var options = new DbContextOptionsBuilder<DispatchManagerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DispatchManagerDbContext(options);

        _testCustomer = Customer.Create("Test Customer", "test@email.com", "+51-999-123-456");
        _testProduct = Product.Create("Test Product", "Test Description", 100m, "units");

        await _context.Customers.AddAsync(_testCustomer);
        await _context.Products.AddAsync(_testProduct);
        await _context.SaveChangesAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task OrderConfiguration_ShouldPersistValueObjectsCorrectly()
    {
        // Arrange
        var origin = Coordinate.Create(-12.046374, -77.042793);
        var destination = Coordinate.Create(-12.066667, -77.116667);
        var distance = Distance.Create(50);
        var cost = DeliveryCost.FromDistance(distance);
        var quantity = Quantity.Create(5);

        var order = Order.Create(
            _testCustomer.Id,
            _testProduct.Id,
            quantity,
            origin,
            destination,
            distance,
            cost);

        // Act
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        var savedOrder = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Product)
            .FirstAsync(o => o.Id == order.Id);

        // Assert
        savedOrder.Should().NotBeNull();

        savedOrder.Origin.Latitude.Should().Be(-12.046374);
        savedOrder.Origin.Longitude.Should().Be(-77.042793);

        savedOrder.Destination.Latitude.Should().Be(-12.066667);
        savedOrder.Destination.Longitude.Should().Be(-77.116667);

        savedOrder.Distance.Kilometers.Should().Be(50);
        savedOrder.Cost.Amount.Should().Be(100m);
        savedOrder.Cost.Currency.Should().Be("USD");
        savedOrder.Quantity.Value.Should().Be(5);

        savedOrder.Status.Should().Be(OrderStatus.Created);

        savedOrder.Customer.Should().NotBeNull();
        savedOrder.Product.Should().NotBeNull();
    }

    [Test]
    public async Task OrderConfiguration_ShouldEnforceRequiredFields()
    {
      

        var act = async () =>
        {
            _context.Database.ExecuteSqlRaw(@"
                INSERT INTO Orders (Id, CustomerId, ProductId, Quantity, 
                    OriginLatitude, OriginLongitude, DestinationLatitude, DestinationLongitude,
                    DistanceKm, CostAmount, CostCurrency, Status, CreatedAt)
                VALUES (NEWID(), NULL, NULL, 1, 0, 0, 0, 0, 1, 100, 'USD', 1, GETUTCDATE())");
        };

        // Assert
        await act.Should().ThrowAsync<Exception>(); 
    }

    [Test]
    public void OrderConfiguration_ShouldHaveCorrectIndexes()
    {
        var entityType = _context.Model.FindEntityType(typeof(Order));

        entityType.Should().NotBeNull();

        var indexes = entityType!.GetIndexes().ToList();
        indexes.Should().NotBeEmpty();

        var customerIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 1 &&
            i.Properties.First().Name == "CustomerId");
        customerIndex.Should().NotBeNull();

        var statusIndex = indexes.FirstOrDefault(i =>
            i.Properties.Count == 1 &&
            i.Properties.First().Name == "Status");
        statusIndex.Should().NotBeNull();
    }
}
