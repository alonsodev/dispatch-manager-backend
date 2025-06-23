using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatchManager.Domain.Services;
using DispatchManager.Domain.ValueObjects;
using DispatchManager.Domain.Exceptions;
using FluentAssertions;
using NUnit.Framework;

namespace DispatchManager.Domain.UnitTests.Services;

[TestFixture]
public sealed class DistanceCalculationServiceTests
{
    private DistanceCalculationService _sut;

    [SetUp]
    public void SetUp()
    {
        _sut = new DistanceCalculationService();
    }

    [Test]
    public void CalculateDistance_BetweenLimaAndCallao_ShouldReturnApproximateDistance()
    {
        // Arrange - Lima centro a Callao (aproximadamente 15 km)
        var lima = Coordinate.Create(-12.046374, -77.042793);
        var callao = Coordinate.Create(-12.066667, -77.116667);

        // Act
        var distance = _sut.CalculateDistance(lima, callao);

        // Assert
        distance.Kilometers.Should().BeInRange(10, 20); // Aproximadamente 15 km
    }

    [Test]
    public void CalculateDistance_BetweenSameCoordinates_ShouldThrowDomainException()
    {
        // Arrange
        var coordinate = Coordinate.Create(-12.046374, -77.042793);

        // Act & Assert
        var act = () => _sut.CalculateDistance(coordinate, coordinate);
        act.Should().Throw<DomainException>(); // Distance.Create no permite 0 km
    }

    [Test]
    public void CalculateDistance_BetweenLimaAndBogota_ShouldThrowDomainException()
    {
        // Arrange - Lima a Bogotá (aproximadamente 1900+ km, excede límite)
        var lima = Coordinate.Create(-12.046374, -77.042793);
        var bogota = Coordinate.Create(4.60971, -74.08175);

        // Act & Assert
        var act = () => _sut.CalculateDistance(lima, bogota);
        act.Should().Throw<DomainException>(); // Excede 1000 km
    }

    [Test]
    public void CalculateDistance_BetweenLimaAndHuancayo_ShouldReturnValidDistance()
    {
        // Arrange - Lima a Huancayo (aproximadamente 300 km)
        var lima = Coordinate.Create(-12.046374, -77.042793);
        var huancayo = Coordinate.Create(-12.065556, -75.204444);

        // Act
        var distance = _sut.CalculateDistance(lima, huancayo);

        // Assert
        distance.Kilometers.Should().BeInRange(200, 400); // Aproximadamente 300 km
        distance.GetCostInterval().Should().Be("201-500 km");
    }
}
