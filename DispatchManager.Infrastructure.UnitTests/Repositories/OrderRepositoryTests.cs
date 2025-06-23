using DispatchManager.Domain.Entities;
using DispatchManager.Domain.Enums;
using DispatchManager.Domain.ValueObjects;
using DispatchManager.Infrastructure.Data;
using DispatchManager.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace DispatchManager.Infrastructure.UnitTests.Repositories;

[TestFixture]
public sealed class OrderRepositoryTests
{
    private DispatchManagerDbContext _context;
    private OrderRepository _repository;
    private Customer _testCustomer;
    private Product _testProduct;

    [SetUp]
    public async Task SetUp()
    {
        var options = new DbContextOptionsBuilder<DispatchManagerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DispatchManagerDbContext(options);
        _repository = new OrderRepository(_context);

        // Create test data
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
    public async Task AddAsync_WithValidOrder_ShouldAddOrderToDatabase()
    {
        // Arrange
        var order = CreateTestOrder();

        // Act
        var result = await _repository.AddAsync(order);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(order.Id);

        var savedOrder = await _context.Orders.FindAsync(order.Id);
        savedOrder.Should().NotBeNull();
        savedOrder!.CustomerId.Should().Be(_testCustomer.Id);
    }

    [Test]
    public async Task GetByIdAsync_WithExistingOrder_ShouldReturnOrder()
    {
        // Arrange
        var order = CreateTestOrder();
        await _repository.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.CustomerId.Should().Be(_testCustomer.Id);
    }

    [Test]
    public async Task GetOrdersByCustomerIdAsync_WithExistingOrders_ShouldReturnCustomerOrders()
    {
        // Arrange
        var order1 = CreateTestOrder();
        var order2 = CreateTestOrder();

        await _repository.AddAsync(order1);
        await _repository.AddAsync(order2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOrdersByCustomerIdAsync(_testCustomer.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(o => o.CustomerId == _testCustomer.Id);
        result.Should().BeInDescendingOrder(o => o.CreatedAt);
    }

    [Test]
    public async Task GetOrdersByStatusAsync_WithSpecificStatus_ShouldReturnOrdersWithThatStatus()
    {
        // Arrange
        var order1 = CreateTestOrder();
        var order2 = CreateTestOrder();
        order2.UpdateStatus(OrderStatus.InProgress);

        await _repository.AddAsync(order1);
        await _repository.AddAsync(order2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOrdersByStatusAsync(OrderStatus.Created);

        // Assert
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(OrderStatus.Created);
    }

    [Test]
    public async Task GetOrderCountByDistanceIntervalAsync_ShouldReturnCorrectGrouping()
    {
        // Arrange
        var order1 = CreateTestOrderWithDistance(25); // 1-50 km
        var order2 = CreateTestOrderWithDistance(100); // 51-200 km
        var order3 = CreateTestOrderWithDistance(30); // 1-50 km

        await _repository.AddAsync(order1);
        await _repository.AddAsync(order2);
        await _repository.AddAsync(order3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOrderCountByDistanceIntervalAsync();

        // Assert
        result.Should().HaveCount(2);
        result["1-50 km"].Should().Be(2);
        result["51-200 km"].Should().Be(1);
    }

    [Test]
    public async Task HasOrdersInProgressForCustomerAsync_WithInProgressOrders_ShouldReturnTrue()
    {
        // Arrange
        var order = CreateTestOrder();
        order.UpdateStatus(OrderStatus.InProgress);

        await _repository.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.HasOrdersInProgressForCustomerAsync(_testCustomer.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task HasOrdersInProgressForCustomerAsync_WithoutInProgressOrders_ShouldReturnFalse()
    {
        // Arrange
        var order = CreateTestOrder();
        order.UpdateStatus(OrderStatus.Delivered);

        await _repository.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.HasOrdersInProgressForCustomerAsync(_testCustomer.Id);

        // Assert
        result.Should().BeFalse();
    }

    private Order CreateTestOrder()
    {
        return CreateTestOrderWithDistance(50);
    }

    private Order CreateTestOrderWithDistance(double kilometers)
    {
        var origin = Coordinate.Create(-12.046374, -77.042793);
        var destination = Coordinate.Create(-12.066667, -77.116667);
        var distance = Distance.Create(kilometers);
        var cost = DeliveryCost.FromDistance(distance);
        var quantity = Quantity.Create(1);

        return Order.Create(
            _testCustomer.Id,
            _testProduct.Id,
            quantity,
            origin,
            destination,
            distance,
            cost);
    }
}