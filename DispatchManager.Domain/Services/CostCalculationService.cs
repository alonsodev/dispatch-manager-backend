using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatchManager.Domain.ValueObjects;

namespace DispatchManager.Domain.Services;

public sealed class CostCalculationService : ICostCalculationService
{
    public DeliveryCost CalculateCost(Distance distance)
    {
        return DeliveryCost.FromDistance(distance);
    }

    public string GetDistanceInterval(Distance distance)
    {
        return distance.Kilometers switch
        {
            >= 1 and <= 50 => "1-50 km",
            >= 51 and <= 200 => "51-200 km",
            >= 201 and <= 500 => "201-500 km",
            >= 501 and <= 1000 => "501-1000 km",
            _ => "Rango Inválido"
        };
    }
}