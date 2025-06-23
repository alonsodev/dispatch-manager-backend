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
public sealed class DistanceTests
{
    [TestCase(1)]
    [TestCase(50)]
    [TestCase(500)]
    [TestCase(1000)]
    public void Create_WithValidDistance_ShouldSucceed(double kilometers)
    {
        // Act
        var distance = Distance.Create(kilometers);

        // Assert
        distance.Kilometers.Should().Be(kilometers);
    }

    [TestCase(0.9)]    // Less than 1 km
    [TestCase(1001)]   // More than 1000 km
    [TestCase(-5)]     // Negative
    public void Create_WithInvalidDistance_ShouldThrowDomainException(double kilometers)
    {
        // Act & Assert
        var act = () => Distance.Create(kilometers);
        act.Should().Throw<DomainException>();
    }

    [TestCase(25, 1, 50, true)]    // In range
    [TestCase(100, 51, 200, true)] // In range
    [TestCase(75, 1, 50, false)]   // Out of range
    public void IsInRange_ShouldReturnExpectedResult(double kilometers, double min, double max, bool expected)
    {
        // Arrange
        var distance = Distance.Create(kilometers);

        // Act
        var result = distance.IsInRange(min, max);

        // Assert
        result.Should().Be(expected);
    }

    [TestCase(25, "1-50 km")]
    [TestCase(100, "51-200 km")]
    [TestCase(300, "201-500 km")]
    [TestCase(750, "501-1000 km")]
    public void GetCostInterval_ShouldReturnCorrectInterval(double kilometers, string expectedInterval)
    {
        // Arrange
        var distance = Distance.Create(kilometers);

        // Act
        var result = distance.GetCostInterval();

        // Assert
        result.Should().Be(expectedInterval);
    }

    [Test]
    public void CompareTo_WithDifferentDistances_ShouldReturnCorrectComparison()
    {
        // Arrange
        var distance1 = Distance.Create(50);
        var distance2 = Distance.Create(100);

        // Act & Assert
        distance1.CompareTo(distance2).Should().BeLessThan(0);
        distance2.CompareTo(distance1).Should().BeGreaterThan(0);
        distance1.CompareTo(distance1).Should().Be(0);

        (distance1 < distance2).Should().BeTrue();
        (distance2 > distance1).Should().BeTrue();
        (distance1 <= distance1).Should().BeTrue();
        (distance2 >= distance2).Should().BeTrue();
    }
}
