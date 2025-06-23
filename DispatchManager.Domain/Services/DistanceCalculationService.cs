using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatchManager.Domain.ValueObjects;

namespace DispatchManager.Domain.Services;

public sealed class DistanceCalculationService : IDistanceCalculationService
{
    private const double EarthRadiusKm = 6371.0;

    public Distance CalculateDistance(Coordinate origin, Coordinate destination)
    {
        var lat1Rad = DegreesToRadians(origin.Latitude);
        var lat2Rad = DegreesToRadians(destination.Latitude);
        var deltaLatRad = DegreesToRadians(destination.Latitude - origin.Latitude);
        var deltaLonRad = DegreesToRadians(destination.Longitude - origin.Longitude);

        var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distanceKm = EarthRadiusKm * c;

        return Distance.Create(distanceKm);
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}