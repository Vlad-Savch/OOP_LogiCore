using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LogiCore.Domain.Models;
namespace LogiCore.Domain.Infrastructure;



public sealed class OrderCreatedEventArgs : EventArgs
{
    public OrderCreatedEventArgs(Order order) => Order = order;
    public Order Order { get; }
}

public sealed class OrderStatusChangedEventArgs : EventArgs
{
    public OrderStatusChangedEventArgs(Order order, OrderStatus oldStatus, OrderStatus newStatus)
    {
        Order = order; OldStatus = oldStatus; NewStatus = newStatus;
    }
    public Order Order { get; }
    public OrderStatus OldStatus { get; }
    public OrderStatus NewStatus { get; }
}

public sealed class VehicleOverloadAttemptEventArgs : EventArgs
{
    public VehicleOverloadAttemptEventArgs(Vehicle vehicle, decimal weightKg, decimal volumeM3)
    { Vehicle = vehicle; WeightKg = weightKg; VolumeM3 = volumeM3; }
    public Vehicle Vehicle { get; }
    public decimal WeightKg { get; }
    public decimal VolumeM3 { get; }
}

public sealed class DeliveryCompletedEventArgs : EventArgs
{
    public DeliveryCompletedEventArgs(Order order, decimal revenue)
    { Order = order; Revenue = revenue; }
    public Order Order { get; }
    public decimal Revenue { get; }
}

public delegate void LogisticsEventHandler<in TEventArgs>(object sender, TEventArgs args) where TEventArgs : EventArgs;

