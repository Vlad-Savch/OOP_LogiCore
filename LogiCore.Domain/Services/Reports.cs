using System;
using LogiCore.Domain.Models;
using LogiCore.Domain.Infrastructure;
using LogiCore.Domain.Patterns;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace LogiCore.Domain.Services;



public sealed class ReportService
{
    private readonly LogisticsState _state;

    public ReportService(LogisticsState state) => _state = state;

    public IEnumerable<(string VehicleType, decimal Revenue)> TopVehiclesByRevenue(int take = 3) =>
        _state.Orders.GetAll()
            .Where(o => o.Status == OrderStatus.Delivered && o.AssignedVehicle is not null)
            .GroupBy(o => o.AssignedVehicle!)
            .Select(g => (VehicleType: g.Key.GetType().Name, Revenue: g.Sum(o => o.TotalCost)))
            .OrderByDescending(x => x.Revenue)
            .Take(take);

    public IEnumerable<(OrderStatus Status, int Count, decimal Total)> OrdersByStatus() =>
        _state.Orders.GetAll()
            .GroupBy(o => o.Status)
            .Select(g => (g.Key, g.Count(), g.Sum(o => o.TotalCost)))
            .OrderBy(x => x.Key);

    public IEnumerable<(string VehicleType, double AverageLoadPercent)> AverageVehicleLoadByType() =>
        _state.Orders.GetAll()
            .Where(o => o.AssignedVehicle is not null)
            .Select(o => new
            {
                Vehicle = o.AssignedVehicle!,
                Load = o.Cargo.Sum(c => c.WeightKg) / o.AssignedVehicle!.MaxLoadKg * 100m
            })
            .GroupBy(x => x.Vehicle.GetType().Name)
            .Select(g => (g.Key, (double)g.Average(x => x.Load)));

    public IEnumerable<(string Customer, decimal Total)> CustomersAbove(decimal threshold) =>
        _state.Orders.GetAll()
            .GroupBy(o => o.Customer)
            .Select(g => (Customer: g.Key.Name, Total: g.Sum(o => o.TotalCost)))
            .Where(x => x.Total > threshold)
            .OrderByDescending(x => x.Total);

    public IEnumerable<(string Cargo, string Order, string Customer)> CargoOrderCustomerJoin() =>
        from cargo in _state.Orders.GetAll().SelectMany(o => o.Cargo, (order, cargo) => new { order, cargo })
        join customer in _state.Customers.GetAll() on cargo.order.Customer.Id equals customer.Id
        select (cargo.cargo.Description, cargo.order.Id.ToString()[..8], customer.Name);

    public IDictionary<int, int> DangerousClassCounts() =>
        _state.Orders.GetAll()
            .SelectMany(o => o.Cargo)
            .OfType<DangerousCargo>()
            .ToLookup(c => c.HazardClass)
            .ToDictionary(g => g.Key, g => g.Count());

    public decimal AverageOrderValue() => _state.Orders.GetAll().Any()
        ? _state.Orders.GetAll().Average(o => o.TotalCost)
        : 0m;
}

