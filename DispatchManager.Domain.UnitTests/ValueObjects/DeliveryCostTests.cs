using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatchManager.Domain.ValueObjects;
using DispatchManager.Domain.Exceptions;
using FluentAssertions;
using NUnit.Framework;

namespace DispatchManager.Domain.UnitTests.ValueObjects;

[TestFixture]
public sealed class DeliveryCostTests
{
    [TestCase(25, 100)]   // 1-50 km range
    [TestCase(100, 300)]  // 51-200 km range
    [TestCase(300, 1000)] // 201-500 km range
    [TestCase(750, 1500)] // 501-1000 km range
    public void FromDistance_ShouldCalculateCorrectCost(double kilometers, decimal expectedCost)
    {
        // Arrange
        var distance = Distance.Create(kilometers);

        // Act
        var cost = DeliveryCost.FromDistance(distance);

        // Assert
        cost.Amount.Should().Be(expectedCost);
        cost.Currency.Should().Be("USD");
    }

    [Test]
    public void Create_WithValidAmount_ShouldSucceed()
    {
        // Act
        var cost = DeliveryCost.Create(100m);

        // Assert
        cost.Amount.Should().Be(100m);
        cost.Currency.Should().Be("USD");
    }

    [Test]
    public void Create_WithNegativeAmount_ShouldThrowDomainException()
    {
        // Act & Assert
        var act = () => DeliveryCost.Create(-10m);
        act.Should().Throw<DomainException>();
    }

    [TestCase(10, 90)]   // 10% discount
    [TestCase(25, 75)]   // 25% discount
    [TestCase(50, 50)]   // 50% discount
    [TestCase(0, 100)]   // No discount
    public void ApplyDiscount_WithValidPercentage_ShouldCalculateCorrectAmount(decimal discountPercentage, decimal expectedAmount)
    {
        // Arrange
        var originalCost = DeliveryCost.Create(100m);

        // Act
        var discountedCost = originalCost.ApplyDiscount(discountPercentage);

        // Assert
        discountedCost.Amount.Should().Be(expectedAmount);
    }

    [TestCase(-5)]   // Negative percentage
    [TestCase(101)]  // Over 100%
    public void ApplyDiscount_WithInvalidPercentage_ShouldThrowDomainException(decimal invalidPercentage)
    {
        // Arrange
        var cost = DeliveryCost.Create(100m);

        // Act & Assert
        var act = () => cost.ApplyDiscount(invalidPercentage);
        act.Should().Throw<DomainException>();
    }

    [Test]
    public void ToString_ShouldReturnFormattedCurrency()
    {
        // Arrange
        var cost = DeliveryCost.Create(150.50m);

        // Act
        var result = cost.ToString();

        // Assert
        result.Should().Contain("150").And.Contain("USD");
    }
}