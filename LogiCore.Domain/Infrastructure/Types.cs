using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace LogiCore.Domain.Infrastructure;


public enum OrderStatus
{
    Created,
    Assigned,
    InTransit,
    Delivered,
    Cancelled
}


[Flags]
public enum TransportConditions
{
    None = 0,
    Refrigerated = 1,
    Sealed = 2,
    Pressurized = 4,
    LongRange = 8
}


public enum VehicleState
{
    Free,
    InTransit,
    UnderMaintenance
}

