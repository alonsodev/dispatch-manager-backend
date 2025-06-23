using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatchManager.Domain.ValueObjects;

namespace DispatchManager.Domain.Services;

public interface IDistanceCalculationService
{
    /// <summary>
    /// Calcula la distancia entre dos coordenadas usando la fórmula de Haversine
    /// </summary>
    /// <param name="origin">Coordenada de origen</param>
    /// <param name="destination">Coordenada de destino</param>
    /// <returns>Distancia en kilómetros</returns>
    Distance CalculateDistance(Coordinate origin, Coordinate destination);
}