using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatchManager.Domain.ValueObjects;

namespace DispatchManager.Domain.Services;

public interface ICostCalculationService
{
    /// <summary>
    /// Calcula el costo de envío basado en la distancia según las reglas de negocio
    /// </summary>
    /// <param name="distance">Distancia del envío</param>
    /// <returns>Costo de envío en USD</returns>
    DeliveryCost CalculateCost(Distance distance);

    /// <summary>
    /// Obtiene el intervalo de distancia al que pertenece una distancia dada
    /// </summary>
    /// <param name="distance">Distancia a evaluar</param>
    /// <returns>Descripción del intervalo (ej: "1-50 km")</returns>
    string GetDistanceInterval(Distance distance);
}
