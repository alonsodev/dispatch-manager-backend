using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatchManager.Domain.ValueObjects;
using DispatchManager.Domain.Enums;
using DispatchManager.Domain.Exceptions;
using DispatchManager.Domain.Events;

namespace DispatchManager.Domain.Entities;

public sealed class Order
{
    private readonly List<OrderDomainEvent> _domainEvents = new();

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ProductId { get; private set; }
    public Quantity Quantity { get; private set; }
    public Coordinate Origin { get; private set; }
    public Coordinate Destination { get; private set; }
    public Distance Distance { get; private set; }
    public DeliveryCost Cost { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public Customer Customer { get; private set; }
    public Product Product { get; private set; }

    public IReadOnlyCollection<OrderDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Order() { } // EF Constructor

    private Order(
        Guid customerId,
        Guid productId,
        Quantity quantity,
        Coordinate origin,
        Coordinate destination,
        Distance distance,
        DeliveryCost cost)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        ProductId = productId;
        Quantity = quantity;
        Origin = origin;
        Destination = destination;
        Distance = distance;
        Cost = cost;
        Status = OrderStatus.Created;
        CreatedAt = DateTime.UtcNow;

        _domainEvents.Add(OrderCreatedDomainEvent.Create(Id, CustomerId, distance.Kilometers, cost.Amount));
    }

    public static Order Create(
        Guid customerId,
        Guid productId,
        Quantity quantity,
        Coordinate origin,
        Coordinate destination,
        Distance distance,
        DeliveryCost cost)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("Se requiere un ID de cliente");

        if (productId == Guid.Empty)
            throw new DomainException("Se requiere un ID de producto");

        if (origin == destination)
            throw new DomainException("El origen y el destino no pueden ser los mismos");

        return new Order(customerId, productId, quantity, origin, destination, distance, cost);
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        if (!CanTransitionTo(newStatus))
            throw new DomainException($"No se puede transitar desde {Status} a {newStatus}");

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private bool CanTransitionTo(OrderStatus newStatus)
    {
        return Status switch
        {
            OrderStatus.Created => newStatus is OrderStatus.InProgress or OrderStatus.Cancelled,
            OrderStatus.InProgress => newStatus is OrderStatus.Sending or OrderStatus.Delivered or OrderStatus.Cancelled,
            OrderStatus.Sending => newStatus is OrderStatus.Delivered,
            OrderStatus.Delivered => false,
            OrderStatus.Cancelled => false,
            _ => false
        };
    }
}