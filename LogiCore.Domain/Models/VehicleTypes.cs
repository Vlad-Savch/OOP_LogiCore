using LogiCore.Domain.Infrastructure;

namespace LogiCore.Domain.Models;

public sealed class Truck : Vehicle
{
    public Truck(Guid id, string registrationNumber, decimal maxLoadKg, decimal maxVolumeM3,
        decimal averageSpeedKmH, decimal baseRatePerKm, decimal tollCoefficient = 0.1m)
        : base(id, registrationNumber, maxLoadKg, maxVolumeM3, averageSpeedKmH, baseRatePerKm)
        => TollCoefficient = tollCoefficient;

    public decimal TollCoefficient { get; }

    public override decimal CalculateDeliveryCost(Route route, IReadOnlyCollection<Cargo> cargo) =>
        BaseRatePerKm * route.DistanceKm * (1 + TollCoefficient);
}


public sealed class RefrigeratorTruck : Vehicle
{
    public RefrigeratorTruck(Guid id, string registrationNumber, decimal maxLoadKg, decimal maxVolumeM3,
        decimal averageSpeedKmH, decimal baseRatePerKm, decimal minTemperatureC, decimal maxTemperatureC)
        : base(id, registrationNumber, maxLoadKg, maxVolumeM3, averageSpeedKmH, baseRatePerKm)
    {
        if (minTemperatureC > maxTemperatureC) throw new ArgumentException("Неверный диапазон температур.");
        MinTemperatureC = minTemperatureC;
        MaxTemperatureC = maxTemperatureC;
    }

    public decimal MinTemperatureC { get; }
    public override bool SupportsTemperatureControl => true;
    public decimal MaxTemperatureC { get; }

    public override bool CanCarry(Cargo cargo) =>
        base.CanCarry(cargo) && (! (cargo is ITemperatureSensitive temp) ||
                                  (temp.RequiredTemperatureC >= MinTemperatureC && temp.RequiredTemperatureC <= MaxTemperatureC));

    public override decimal CalculateDeliveryCost(Route route, IReadOnlyCollection<Cargo> cargo) =>
        BaseRatePerKm * route.DistanceKm * 1.25m;
}


public sealed class CargoPlane : Vehicle
{
    public CargoPlane(Guid id, string registrationNumber, decimal maxLoadKg, decimal maxVolumeM3,
        decimal averageSpeedKmH, decimal baseRatePerKm)
        : base(id, registrationNumber, maxLoadKg, maxVolumeM3, averageSpeedKmH, baseRatePerKm) { }

    public override bool CanCarry(Cargo cargo) =>
        base.CanCarry(cargo) && (cargo is not DangerousCargo dangerous || dangerous.HazardClass <= 3);

    public override decimal CalculateDeliveryCost(Route route, IReadOnlyCollection<Cargo> cargo) =>
        BaseRatePerKm * route.DistanceKm * 1.8m + cargo.Sum(c => c.WeightKg) * 0.12m;
}


public sealed class CargoShip : Vehicle
{
    public CargoShip(Guid id, string registrationNumber, decimal maxLoadKg, decimal maxVolumeM3,
        decimal averageSpeedKmH, decimal baseRatePerKm)
        : base(id, registrationNumber, maxLoadKg, maxVolumeM3, averageSpeedKmH, baseRatePerKm) { }

    public override decimal CalculateDeliveryCost(Route route, IReadOnlyCollection<Cargo> cargo) =>
        BaseRatePerKm * route.DistanceKm * 0.6m + cargo.OfType<OversizedCargo>().Count() * 500m;
}


public sealed class DroneCourier : Vehicle
{
    public DroneCourier(Guid id, string registrationNumber, decimal averageSpeedKmH, decimal baseRatePerKm)
        : base(id, registrationNumber, 20m, 0.15m, averageSpeedKmH, baseRatePerKm) { }

    public override decimal MaxRangeKm => 50m;

    public override decimal CalculateDeliveryCost(Route route, IReadOnlyCollection<Cargo> cargo) =>
        BaseRatePerKm * route.DistanceKm + 2m * cargo.Sum(c => c.WeightKg);
}


