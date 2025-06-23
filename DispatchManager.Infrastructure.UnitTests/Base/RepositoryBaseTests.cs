using DispatchManager.Domain.Entities;
using DispatchManager.Infrastructure.Data;
using DispatchManager.Infrastructure.Repositories.Base;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace DispatchManager.Infrastructure.UnitTests.Base;

[TestFixture]
public sealed class RepositoryBaseTests
{
    private DispatchManagerDbContext _context;
    private Repository<Customer> _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<DispatchManagerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DispatchManagerDbContext(options);
        _repository = new Repository<Customer>(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task GetPagedAsync_WithValidParameters_ShouldReturnCorrectPage()
    {
        // Arrange
        var customers = new List<Customer>();
        for (int i = 1; i <= 10; i++)
        {
            customers.Add(Customer.Create($"Customer {i:D2}", $"customer{i}@email.com", $"+51-999-123-{i:D3}"));
        }

        await _repository.AddRangeAsync(customers);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPagedAsync(
            pageNumber: 2,
            pageSize: 3,
            orderBy: c => c.Name,
            ascending: true);

        // Assert
        result.TotalCount.Should().Be(10);
        result.Items.Should().HaveCount(3);
        result.Items.First().Name.Should().Be("Customer 04");
        result.Items.Last().Name.Should().Be("Customer 06");
    }

    [Test]
    public async Task FindAsync_WithPredicate_ShouldReturnMatchingEntities()
    {
        // Arrange
        var customer1 = Customer.Create("John Smith", "john@email.com", "+51-999-123-456");
        var customer2 = Customer.Create("Jane Smith", "jane@email.com", "+51-999-234-567");
        var customer3 = Customer.Create("Bob Johnson", "bob@email.com", "+51-999-345-678");

        await _repository.AddAsync(customer1);
        await _repository.AddAsync(customer2);
        await _repository.AddAsync(customer3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindAsync(c => c.Name.Contains("Smith"));

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.Name.Contains("Smith"));
    }

    [Test]
    public async Task ExistsAsync_WithExistingEntity_ShouldReturnTrue()
    {
        // Arrange
        var customer = Customer.Create("Test Customer", "test@email.com", "+51-999-123-456");
        await _repository.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(c => c.Email == "test@email.com");

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task CountAsync_WithPredicate_ShouldReturnCorrectCount()
    {
        // Arrange
        var customer1 = Customer.Create("Active User", "active1@email.com", "+51-999-123-456");
        var customer2 = Customer.Create("Active User", "active2@email.com", "+51-999-234-567");
        var customer3 = Customer.Create("Inactive User", "inactive@email.com", "+51-999-345-678");

        await _repository.AddAsync(customer1);
        await _repository.AddAsync(customer2);
        await _repository.AddAsync(customer3);
        await _context.SaveChangesAsync();

        // Act
        var activeCount = await _repository.CountAsync(c => c.Name.Contains("Active"));
        var totalCount = await _repository.CountAsync();

        // Assert
        activeCount.Should().Be(2);
        totalCount.Should().Be(3);
    }

    [Test]
    public async Task RemoveByIdAsync_WithExistingEntity_ShouldRemoveEntity()
    {
        // Arrange
        var customer = Customer.Create("Test Customer", "test@email.com", "+51-999-123-456");
        await _repository.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Act
        await _repository.RemoveByIdAsync(customer.Id);
        await _context.SaveChangesAsync();

        // Assert
        var deletedCustomer = await _repository.GetByIdAsync(customer.Id);
        deletedCustomer.Should().BeNull();
    }
}