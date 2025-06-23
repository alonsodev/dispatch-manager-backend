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
public sealed class QuantityTests
{
    [TestCase(1)]
    [TestCase(10)]
    [TestCase(100)]
    [TestCase(1000)]
    public void Create_WithValidQuantity_ShouldSucceed(int value)
    {
        // Act
        var quantity = Quantity.Create(value);

        // Assert
        quantity.Value.Should().Be(value);
        ((int)quantity).Should().Be(value); // Test implicit conversion
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-10)]
    public void Create_WithInvalidQuantity_ShouldThrowDomainException(int value)
    {
        // Act & Assert
        var act = () => Quantity.Create(value);
        act.Should().Throw<DomainException>();
    }

    [TestCase(5, 3, 8)]
    [TestCase(10, 5, 15)]
    [TestCase(1, 1, 2)]
    public void Add_WithValidIncrement_ShouldReturnNewQuantity(int initial, int increment, int expected)
    {
        // Arrange
        var quantity = Quantity.Create(initial);

        // Act
        var result = quantity.Add(increment);

        // Assert
        result.Value.Should().Be(expected);
        quantity.Value.Should().Be(initial); // Original should be unchanged
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Add_WithInvalidIncrement_ShouldThrowDomainException(int increment)
    {
        // Arrange
        var quantity = Quantity.Create(10);

        // Act & Assert
        var act = () => quantity.Add(increment);
        act.Should().Throw<DomainException>();
    }

    [TestCase(10, 3, 7)]
    [TestCase(5, 2, 3)]
    [TestCase(100, 50, 50)]
    public void Subtract_WithValidDecrement_ShouldReturnNewQuantity(int initial, int decrement, int expected)
    {
        // Arrange
        var quantity = Quantity.Create(initial);

        // Act
        var result = quantity.Subtract(decrement);

        // Assert
        result.Value.Should().Be(expected);
    }

    [TestCase(5, 5)]  // Equal to current value
    [TestCase(5, 6)]  // Greater than current value
    [TestCase(5, 0)]  // Zero decrement
    [TestCase(5, -1)] // Negative decrement
    public void Subtract_WithInvalidDecrement_ShouldThrowDomainException(int initial, int decrement)
    {
        // Arrange
        var quantity = Quantity.Create(initial);

        // Act & Assert
        var act = () => quantity.Subtract(decrement);
        act.Should().Throw<DomainException>();
    }

    [Test]
    public void CompareTo_WithDifferentQuantities_ShouldReturnCorrectComparison()
    {
        // Arrange
        var quantity1 = Quantity.Create(5);
        var quantity2 = Quantity.Create(10);

        // Act & Assert
        quantity1.CompareTo(quantity2).Should().BeLessThan(0);
        quantity2.CompareTo(quantity1).Should().BeGreaterThan(0);
        quantity1.CompareTo(quantity1).Should().Be(0);

        (quantity1 < quantity2).Should().BeTrue();
        (quantity2 > quantity1).Should().BeTrue();
        (quantity1 <= quantity1).Should().BeTrue();
        (quantity2 >= quantity2).Should().BeTrue();
    }

    [Test]
    public void ExplicitConversion_FromInt_ShouldCreateQuantity()
    {
        // Act
        var quantity = (Quantity)5;

        // Assert
        quantity.Value.Should().Be(5);
    }
}