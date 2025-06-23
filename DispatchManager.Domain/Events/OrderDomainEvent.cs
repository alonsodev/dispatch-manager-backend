using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchManager.Domain.Events;

public abstract class OrderDomainEvent
{
    public Guid Id { get; protected set; }
    public DateTime OccurredOn { get; protected set; }

    protected OrderDomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}