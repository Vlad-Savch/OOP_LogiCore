using System;
using LogiCore.Domain.Models;
using LogiCore.Domain.Infrastructure;
using LogiCore.Domain.Patterns;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace LogiCore.Domain.Services;



public sealed class DeliveryService
{
    private readonly CargoCompatibilityValidator _validator;
    private readonly TariffRegistry _tariffs;

    public DeliveryService(LogisticsState state, CargoCompatibilityValidator validator, TariffRegistry tariffs, decimal initialRevenue = 0m)
    {
        State = state;
        _validator = validator;
        _tariffs = tariffs;
        Revenue = initialRevenue;
    }

    public LogisticsState State { get; }
    public decimal Revenue { get; private set; }

    public event LogisticsEventHandler<OrderCreatedEventArgs>? OrderCreated;
    public event LogisticsEventHandler<OrderStatusChangedEventArgs>? OrderStatusChanged;
    public event LogisticsEventHandler<VehicleOverloadAttemptEventArgs>? VehicleOverloadAttempt;
    public event LogisticsEventHandler<DeliveryCompletedEventArgs>? DeliveryCompleted;

    public Order AcceptOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        _validator.ValidateOrThrow(order.Cargo);
        State.Orders.Add(order);
        Raise(OrderCreated, new OrderCreatedEventArgs(order));
        return order;
    }

    public Vehicle AssignCheapestVehicle(Order order, ITariffStrategy tariff, bool insurance = false, bool urgent = false, bool fragilePackaging = false)
    {
        EnsureState(order, OrderStatus.Created);
        var candidates = State.Vehicles.GetAll()
            .Where(v => v.State == VehicleState.Free)
            .Where(v => v.RouteCanHandle(order.Route))
            .Where(v => order.Cargo.All(v.CanCarry))
            .Where(v => order.Cargo.Sum(c => c.WeightKg) <= v.MaxLoadKg)
            .Where(v => order.Cargo.Sum(c => c.VolumeM3) <= v.MaxVolumeM3)
            .ToList();

        if (candidates.Count == 0) throw new VehicleOverloadException("Не найдено свободное совместимое ТС.");

        var selected = candidates.OrderBy(v => v.CalculateDeliveryCost(order.Route, order.Cargo)).First();
        var effectiveTariff = _tariffs.Get(tariff.Name);
        decimal baseCost = selected.CalculateDeliveryCost(order.Route, order.Cargo);
        decimal tariffCost = effectiveTariff.Calculate(baseCost, order.Route, order.Cargo);
        IDeliveryCost cost = new BaseDeliveryCost(tariffCost, effectiveTariff.Name);
        if (insurance) cost = new InsuranceDecorator(cost, order.Cargo);
        if (urgent) cost = new UrgencyDecorator(cost);
        if (fragilePackaging && order.Cargo.Any(c => c is FragileCargo)) cost = new FragilePackagingDecorator(cost);

        var old = order.Status;
        order.Assign(selected, cost.Total);
        Raise(OrderStatusChanged, new OrderStatusChangedEventArgs(order, old, order.Status));
        return selected;
    }

    public IDeliveryCost BuildDecoratedCost(Order order, ITariffStrategy tariff, bool insurance, bool urgent, bool fragilePackaging)
    {
        var vehicle = order.AssignedVehicle ?? throw new InvalidOperationException("Сначала назначьте ТС.");
        var effectiveTariff = _tariffs.Get(tariff.Name);
        decimal baseCost = vehicle.CalculateDeliveryCost(order.Route, order.Cargo);
        decimal tariffCost = effectiveTariff.Calculate(baseCost, order.Route, order.Cargo);
        IDeliveryCost result = new BaseDeliveryCost(tariffCost, effectiveTariff.Name);
        if (insurance) result = new InsuranceDecorator(result, order.Cargo);
        if (urgent) result = new UrgencyDecorator(result);
        if (fragilePackaging && order.Cargo.Any(c => c is FragileCargo)) result = new FragilePackagingDecorator(result);
        return result;
    }

    public void StartDelivery(Order order)
    {
        EnsureState(order, OrderStatus.Assigned);
        var old = order.Status;
        order.AssignedVehicle!.MarkInTransit();
        order.StartDelivery();
        Raise(OrderStatusChanged, new OrderStatusChangedEventArgs(order, old, order.Status));
    }

    public void CompleteDelivery(Order order)
    {
        EnsureState(order, OrderStatus.InTransit);
        var old = order.Status;
        order.Complete();
        order.AssignedVehicle!.MarkFree();
        Revenue += order.TotalCost;
        Raise(OrderStatusChanged, new OrderStatusChangedEventArgs(order, old, order.Status));
        Raise(DeliveryCompleted, new DeliveryCompletedEventArgs(order, order.TotalCost));
    }

    public void CancelOrder(Order order)
    {
        var old = order.Status;
        order.Cancel();
        if (order.AssignedVehicle is { State: VehicleState.InTransit }) order.AssignedVehicle.MarkFree();
        Raise(OrderStatusChanged, new OrderStatusChangedEventArgs(order, old, order.Status));
    }

    public void AttemptOverload(Vehicle vehicle, IReadOnlyCollection<Cargo> cargo)
    {
        decimal weight = cargo.Sum(c => c.WeightKg);
        decimal volume = cargo.Sum(c => c.VolumeM3);
        if (weight > vehicle.MaxLoadKg || volume > vehicle.MaxVolumeM3)
        {
            Raise(VehicleOverloadAttempt, new VehicleOverloadAttemptEventArgs(vehicle, weight, volume));
            throw new VehicleOverloadException($"Попытка перегруза {vehicle.RegistrationNumber}.");
        }
        _validator.ValidateForVehicleOrThrow(vehicle, cargo);
    }

    private static void EnsureState(Order order, OrderStatus expected)
    {
        if (order.Status != expected) throw new InvalidOrderStateException($"Требуется {expected}, получено {order.Status}.");
    }

    private void Raise<T>(LogisticsEventHandler<T>? handler, T args) where T : EventArgs => handler?.Invoke(this, args);
}

internal static class VehicleRouteExtensions
{
    public static bool RouteCanHandle(this Vehicle vehicle, Route route) => (decimal)route.DistanceKm <= vehicle.MaxRangeKm;
}

