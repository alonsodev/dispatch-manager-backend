using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatchManager.Domain.Services;
using DispatchManager.Domain.ValueObjects;
using FluentAssertions;
using NUnit.Framework;

namespace DispatchManager.Domain.UnitTests.Services;

[TestFixture]
public sealed class CostCalculationServiceTests
{
    private CostCalculationService _sut;

    [SetUp]
    public void SetUp()
    {
        _sut = new CostCalculationService();
    }

    [TestCase(25, 100)]   // 1-50 km
    [TestCase(100, 300)]  // 51-200 km
    [TestCase(300, 1000)] // 201-500 km
    [TestCase(750, 1500)] // 501-1000 km
    public void CalculateCost_ShouldReturnCorrectCostForDistance(double kilometers, decimal expectedCost)
    {
        // Arrange
        var distance = Distance.Create(kilometers);

        // Act
        var cost = _sut.CalculateCost(distance);

        // Assert
        cost.Amount.Should().Be(expectedCost);
        cost.Currency.Should().Be("USD");
    }

    [TestCase(25, "1-50 km")]
    [TestCase(100, "51-200 km")]
    [TestCase(300, "201-500 km")]
    [TestCase(750, "501-1000 km")]
    public void GetDistanceInterval_ShouldReturnCorrectInterval(double kilometers, string expectedInterval)
    {
        // Arrange
        var distance = Distance.Create(kilometers);

        // Act
        var result = _sut.GetDistanceInterval(distance);

        // Assert
        result.Should().Be(expectedInterval);
    }
}