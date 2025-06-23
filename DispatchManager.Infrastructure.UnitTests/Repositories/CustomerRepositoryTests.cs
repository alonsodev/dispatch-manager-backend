using DispatchManager.Domain.Entities;
using DispatchManager.Infrastructure.Data;
using DispatchManager.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace DispatchManager.Infrastructure.UnitTests.Repositories;

[TestFixture]
public sealed class CustomerRepositoryTests
{
    private DispatchManagerDbContext _context;
    private CustomerRepository _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<DispatchManagerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DispatchManagerDbContext(options);
        _repository = new CustomerRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnCustomer()
    {
        // Arrange
        var customer = Customer.Create("John Doe", "john@email.com", "+51-999-123-456");
        await _repository.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("john@email.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("john@email.com");
        result.Name.Should().Be("John Doe");
    }

    [Test]
    public async Task GetByEmailAsync_WithNonExistingEmail_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByEmailAsync("nonexistent@email.com");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ExistsByEmailAsync_WithExistingEmail_ShouldReturnTrue()
    {
        // Arrange
        var customer = Customer.Create("Jane Doe", "jane@email.com", "+51-999-234-567");
        await _repository.AddAsync(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByEmailAsync("jane@email.com");

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task ExistsByEmailAsync_WithNonExistingEmail_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsByEmailAsync("nonexistent@email.com");

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task SearchByNameAsync_WithPartialName_ShouldReturnMatchingCustomers()
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
        var result = await _repository.SearchByNameAsync("Smith");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.Name.Contains("Smith"));
        result.Should().BeInAscendingOrder(c => c.Name);
    }

    [Test]
    public async Task GetCustomerListAsync_ShouldReturnOrderedCustomersList()
    {
        // Arrange
        var customer1 = Customer.Create("Charlie Brown", "charlie@email.com", "+51-999-123-456");
        var customer2 = Customer.Create("Alice Cooper", "alice@email.com", "+51-999-234-567");
        var customer3 = Customer.Create("Bob Dylan", "bob@email.com", "+51-999-345-678");

        await _repository.AddAsync(customer1);
        await _repository.AddAsync(customer2);
        await _repository.AddAsync(customer3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCustomerListAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeInAscendingOrder(x => x.Name);
        result.First().Name.Should().Be("Alice Cooper");
        result.Last().Name.Should().Be("Charlie Brown");
    }
}
