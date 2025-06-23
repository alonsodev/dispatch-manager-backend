using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchManager.Domain.Enums;

public enum OrderStatus
{
    Created = 0,
    InProgress = 1,
    Sending = 2,
    Delivered = 3,
    Cancelled = 4
}