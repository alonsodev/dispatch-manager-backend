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
public sealed class CoordinateTests
{
    [TestCase(-12.046374, -77.042793)] // Lima, Peru
    [TestCase(40.7128, -74.0060)]      // New York, USA
    [TestCase(0, 0)]                   // Null Island
    [TestCase(-90, -180)]              // Min values
    [TestCase(90, 180)]                // Max values
    public void Create_WithValidCoordinates_ShouldSucceed(double latitude, double longitude)
    {
        // Act
        var coordinate = Coordinate.Create(latitude, longitude);

        // Assert
        coordinate.Latitude.Should().Be(latitude);
        coordinate.Longitude.Should().Be(longitude);
    }

    [TestCase(-91, 0)]    // Invalid latitude (too low)
    [TestCase(91, 0)]     // Invalid latitude (too high)
    [TestCase(0, -181)]   // Invalid longitude (too low)
    [TestCase(0, 181)]    // Invalid longitude (too high)
    public void Create_WithInvalidCoordinates_ShouldThrowDomainException(double latitude, double longitude)
    {
        // Act & Assert
        var act = () => Coordinate.Create(latitude, longitude);
        act.Should().Throw<DomainException>();
    }

    [Test]
    public void Equals_WithSameCoordinates_ShouldReturnTrue()
    {
        // Arrange
        var coordinate1 = Coordinate.Create(-12.046374, -77.042793);
        var coordinate2 = Coordinate.Create(-12.046374, -77.042793);

        // Act & Assert
        coordinate1.Should().Be(coordinate2);
        (coordinate1 == coordinate2).Should().BeTrue();
    }

    [Test]
    public void Equals_WithDifferentCoordinates_ShouldReturnFalse()
    {
        // Arrange
        var lima = Coordinate.Create(-12.046374, -77.042793);
        var newYork = Coordinate.Create(40.7128, -74.0060);

        // Act & Assert
        lima.Should().NotBe(newYork);
        (lima != newYork).Should().BeTrue();
    }

    [Test]
    public void ToString_ShouldReturnFormattedCoordinates()
    {
        // Arrange
        var coordinate = Coordinate.Create(-12.046374, -77.042793);

        // Act
        var result = coordinate.ToString();

        // Assert
        result.Should().Be("(-12.046374, -77.042793)");
    }
}
