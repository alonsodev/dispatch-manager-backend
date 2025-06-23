using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchManager.Domain.Events;

public sealed class OrderCreatedDomainEvent : OrderDomainEvent
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public double DistanceKm { get; private set; }
    public decimal CostAmount { get; private set; }

    private OrderCreatedDomainEvent(Guid orderId, Guid customerId, double distanceKm, decimal costAmount)
    {
        OrderId = orderId;
        CustomerId = customerId;
        DistanceKm = distanceKm;
        CostAmount = costAmount;
    }

    public static OrderCreatedDomainEvent Create(Guid orderId, Guid customerId, double distanceKm, decimal costAmount)
    {
        return new OrderCreatedDomainEvent(orderId, customerId, distanceKm, costAmount);
    }
}