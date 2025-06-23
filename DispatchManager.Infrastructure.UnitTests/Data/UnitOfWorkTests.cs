using DispatchManager.Domain.Entities;
using DispatchManager.Domain.ValueObjects;
using DispatchManager.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace DispatchManager.Infrastructure.UnitTests.Data;

[TestFixture]
public sealed class UnitOfWorkTests
{
    private DispatchManagerDbContext _context;
    private UnitOfWork _unitOfWork;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<DispatchManagerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DispatchManagerDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
        _context.Dispose();
    }

    [Test]
    public void Repositories_ShouldReturnCorrectRepositoryInstances()
    {
        // Act & Assert
        _unitOfWork.Orders.Should().NotBeNull();
        _unitOfWork.Customers.Should().NotBeNull();
        _unitOfWork.Products.Should().NotBeNull();

        // Verificar que son singleton (misma instancia)
        var orders1 = _unitOfWork.Orders;
        var orders2 = _unitOfWork.Orders;
        orders1.Should().BeSameAs(orders2);
    }

    [Test]
    public async Task SaveChangesAsync_WithValidChanges_ShouldPersistData()
    {
        // Arrange
        var customer = Customer.Create("Test Customer", "test@email.com", "+51-999-123-456");
        await _unitOfWork.Customers.AddAsync(customer);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(1); // One entity saved

        var savedCustomer = await _unitOfWork.Customers.GetByIdAsync(customer.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer!.Name.Should().Be("Test Customer");
    }

    [Test]
    public async Task Transaction_WithCommit_ShouldPersistAllChanges()
    {
        // Arrange
        var customer = Customer.Create("Test Customer", "test@email.com", "+51-999-123-456");
        var product = Product.Create("Test Product", "Description", 100m, "units");

        // Act
        await _unitOfWork.BeginTransactionAsync();

        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        await _unitOfWork.CommitTransactionAsync();

        // Assert
        var savedCustomer = await _unitOfWork.Customers.GetByIdAsync(customer.Id);
        var savedProduct = await _unitOfWork.Products.GetByIdAsync(product.Id);

        savedCustomer.Should().NotBeNull();
        savedProduct.Should().NotBeNull();
    }

    [Test]
    public async Task Transaction_WithRollback_ShouldNotPersistChanges()
    {
        // Arrange
        var customer = Customer.Create("Test Customer", "test@email.com", "+51-999-123-456");

        // Act
        await _unitOfWork.BeginTransactionAsync();

        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        await _unitOfWork.RollbackTransactionAsync();

        // Create new UnitOfWork to check persistence
        using var newUnitOfWork = new UnitOfWork(_context);
        var savedCustomer = await newUnitOfWork.Customers.GetByIdAsync(customer.Id);

        // Assert
        savedCustomer.Should().BeNull();
    }

    [Test]
    public async Task BeginTransactionAsync_WhenTransactionAlreadyExists_ShouldThrowException()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync();

        // Act & Assert
        var act = async () => await _unitOfWork.BeginTransactionAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A transaction is already in progress.");
    }

    [Test]
    public async Task CommitTransactionAsync_WhenNoTransactionExists_ShouldThrowException()
    {
        // Act & Assert
        var act = async () => await _unitOfWork.CommitTransactionAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No transaction in progress to commit.");
    }

    [Test]
    public async Task ExecuteBulkDeleteAsync_WithValidPredicate_ShouldDeleteMatchingEntities()
    {
        // Arrange
        var customer1 = Customer.Create("Customer 1", "customer1@email.com", "+51-999-123-456");
        var customer2 = Customer.Create("Customer 2", "customer2@email.com", "+51-999-234-567");
        var customer3 = Customer.Create("Different Name", "customer3@email.com", "+51-999-345-678");

        await _unitOfWork.Customers.AddAsync(customer1);
        await _unitOfWork.Customers.AddAsync(customer2);
        await _unitOfWork.Customers.AddAsync(customer3);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var deletedCount = await _unitOfWork.ExecuteBulkDeleteAsync<Customer>(c => c.Name.Contains("Customer"));

        // Assert
        deletedCount.Should().Be(2);

        var remainingCustomers = await _unitOfWork.Customers.GetAllAsync();
        remainingCustomers.Should().HaveCount(1);
        remainingCustomers.First().Name.Should().Be("Different Name");
    }
}