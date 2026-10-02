using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace LogiCore.Domain.Infrastructure;



public sealed class StateSnapshot
{
    public decimal Revenue { get; set; }
    public List<VehicleSnapshot> Vehicles { get; set; } = new();
    public List<CustomerSnapshot> Customers { get; set; } = new();
    public List<CargoSnapshot> Cargo { get; set; } = new();
    public List<OrderSnapshot> Orders { get; set; } = new();
}

public sealed class VehicleSnapshot
{
    public Guid Id { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public decimal MaxLoadKg { get; set; }
    public decimal MaxVolumeM3 { get; set; }
    public decimal AverageSpeedKmH { get; set; }
    public decimal BaseRatePerKm { get; set; }
    public decimal TollCoefficient { get; set; }
    public decimal MinTemperatureC { get; set; }
    public decimal MaxTemperatureC { get; set; }
}

public sealed class CustomerSnapshot
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
}

public sealed class CargoSnapshot
{
    public Guid Id { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public decimal VolumeM3 { get; set; }
    public decimal DeclaredValue { get; set; }
    public DateTime ExpirationDate { get; set; }
    public decimal RequiredTemperatureC { get; set; }
    public decimal RiskCoefficient { get; set; }
    public bool CanBeStacked { get; set; }
    public int HazardClass { get; set; }
    public decimal LengthM { get; set; }
    public decimal WidthM { get; set; }
    public decimal HeightM { get; set; }
}

public sealed class OrderSnapshot
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public List<Guid> CargoIds { get; set; } = new();
    public List<RoutePointSnapshot> Route { get; set; } = new();
    public Guid? VehicleId { get; set; }
    public decimal TotalCost { get; set; }
    public OrderStatus Status { get; set; }
}

public sealed class RoutePointSnapshot
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Name { get; set; } = string.Empty;
}

