using System;
using LogiCore.Domain.Models;
using LogiCore.Domain.Infrastructure;
using LogiCore.Domain.Patterns;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace LogiCore.Domain.Services;



public sealed class StatePersistenceService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public void Save(string path, LogisticsState state, decimal revenue)
    {
        ArgumentNullException.ThrowIfNull(state);
        var snapshot = BuildSnapshot(state, revenue);
        using var stream = File.Create(path);
        JsonSerializer.Serialize(stream, snapshot, Options);
    }

    public (LogisticsState State, decimal Revenue) Load(string path)
    {
        if (!File.Exists(path)) return (new LogisticsState(), 0m);
        try
        {
            using var stream = File.OpenRead(path);
            var snapshot = JsonSerializer.Deserialize<StateSnapshot>(stream, Options)
                ?? throw new LogisticsSerializationException("JSON-файл пуст или не содержит снимок состояния.", new InvalidDataException());
            return RestoreSnapshot(snapshot);
        }
        catch (JsonException ex)
        {
            throw new LogisticsSerializationException("JSON-файл повреждён или имеет неверный формат.", ex);
        }
        catch (LogisticsException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new LogisticsSerializationException("Не удалось загрузить состояние.", ex);
        }
    }

    private static StateSnapshot BuildSnapshot(LogisticsState state, decimal revenue)
    {
        var snapshot = new StateSnapshot { Revenue = revenue };
        snapshot.Vehicles = state.Vehicles.GetAll().Select(v => new VehicleSnapshot
        {
            Id = v.Id,
            Kind = v.GetType().Name,
            RegistrationNumber = v.RegistrationNumber,
            MaxLoadKg = v.MaxLoadKg,
            MaxVolumeM3 = v.MaxVolumeM3,
            AverageSpeedKmH = v.AverageSpeedKmH,
            BaseRatePerKm = v.BaseRatePerKm,
            TollCoefficient = v is Truck truck ? truck.TollCoefficient : 0,
            MinTemperatureC = v is RefrigeratorTruck fridge ? fridge.MinTemperatureC : 0,
            MaxTemperatureC = v is RefrigeratorTruck fridge2 ? fridge2.MaxTemperatureC : 0
        }).ToList();

        snapshot.Customers = state.Customers.GetAll().Select(c => new CustomerSnapshot { Id = c.Id, Name = c.Name, Contact = c.Contact }).ToList();
        snapshot.Cargo = state.Orders.GetAll().SelectMany(o => o.Cargo).GroupBy(c => c.Id).Select(g => g.First()).Select(ToSnapshot).ToList();
        snapshot.Orders = state.Orders.GetAll().Select(o => new OrderSnapshot
        {
            Id = o.Id,
            CustomerId = o.Customer.Id,
            CargoIds = o.Cargo.Select(c => c.Id).ToList(),
            Route = o.Route.Points.Select(p => new RoutePointSnapshot { Latitude = p.Latitude, Longitude = p.Longitude, Name = p.Name }).ToList(),
            VehicleId = o.AssignedVehicle?.Id,
            TotalCost = o.TotalCost,
            Status = o.Status
        }).ToList();
        return snapshot;
    }

    private static CargoSnapshot ToSnapshot(Cargo c) => new()
    {
        Id = c.Id,
        Kind = c.GetType().Name,
        Description = c.Description,
        WeightKg = c.WeightKg,
        VolumeM3 = c.VolumeM3,
        DeclaredValue = c.DeclaredValue,
        ExpirationDate = c is PerishableCargo p ? p.ExpirationDate : default,
        RequiredTemperatureC = c is PerishableCargo p2 ? p2.RequiredTemperatureC : 0,
        RiskCoefficient = c is FragileCargo f ? f.RiskCoefficient : 0,
        CanBeStacked = c is IStackable stackable ? stackable.CanBeStacked : false,
        HazardClass = c is DangerousCargo d ? d.HazardClass : 0,
        LengthM = c is OversizedCargo o ? o.LengthM : 0,
        WidthM = c is OversizedCargo o2 ? o2.WidthM : 0,
        HeightM = c is OversizedCargo o3 ? o3.HeightM : 0
    };

    private static (LogisticsState State, decimal Revenue) RestoreSnapshot(StateSnapshot snapshot)
    {
        var state = new LogisticsState();
        foreach (var v in snapshot.Vehicles) state.Vehicles.Add(CreateVehicle(v));
        foreach (var c in snapshot.Customers) state.Customers.Add(new Customer(c.Id, c.Name, c.Contact));
        var cargoById = new Dictionary<Guid, Cargo>();
        foreach (var c in snapshot.Cargo)
        {
            var cargo = CreateCargo(c);
            cargoById[cargo.Id] = cargo;
        }
        foreach (var o in snapshot.Orders)
        {
            var customer = state.Customers[o.CustomerId] ?? throw new RouteNotFoundException($"Клиент {o.CustomerId} не найден.");
            var cargo = o.CargoIds.Select(id => cargoById.TryGetValue(id, out var value) ? value : throw new CargoValidationException($"Груз {id} не найден.")).ToList();
            var route = new Route(o.Route.Select(p => new RoutePoint(p.Latitude, p.Longitude, p.Name)));
            var vehicle = o.VehicleId is null ? null : state.Vehicles[o.VehicleId.Value] ?? throw new RouteNotFoundException($"ТС {o.VehicleId} не найдено.");
            var order = new Order(o.Id, customer, cargo, route);
            order.RestoreState(o.Status, vehicle, o.TotalCost);
            state.Orders.Add(order);
        }
        return (state, snapshot.Revenue);
    }

    private static Vehicle CreateVehicle(VehicleSnapshot v) => v.Kind switch
    {
        nameof(Truck) => new Truck(v.Id, v.RegistrationNumber, v.MaxLoadKg, v.MaxVolumeM3, v.AverageSpeedKmH, v.BaseRatePerKm, v.TollCoefficient),
        nameof(RefrigeratorTruck) => new RefrigeratorTruck(v.Id, v.RegistrationNumber, v.MaxLoadKg, v.MaxVolumeM3, v.AverageSpeedKmH, v.BaseRatePerKm, v.MinTemperatureC, v.MaxTemperatureC),
        nameof(CargoPlane) => new CargoPlane(v.Id, v.RegistrationNumber, v.MaxLoadKg, v.MaxVolumeM3, v.AverageSpeedKmH, v.BaseRatePerKm),
        nameof(CargoShip) => new CargoShip(v.Id, v.RegistrationNumber, v.MaxLoadKg, v.MaxVolumeM3, v.AverageSpeedKmH, v.BaseRatePerKm),
        nameof(DroneCourier) => new DroneCourier(v.Id, v.RegistrationNumber, v.AverageSpeedKmH, v.BaseRatePerKm),
        _ => throw new LogisticsSerializationException($"Неизвестный тип ТС '{v.Kind}'.", new InvalidDataException())
    };

    private static Cargo CreateCargo(CargoSnapshot c) => c.Kind switch
    {
        nameof(StandardCargo) => new StandardCargo(c.Id, c.Description, c.WeightKg, c.VolumeM3, c.DeclaredValue),
        nameof(PerishableCargo) => new PerishableCargo(c.Id, c.Description, c.WeightKg, c.VolumeM3, c.DeclaredValue, c.ExpirationDate, c.RequiredTemperatureC),
        nameof(FragileCargo) => new FragileCargo(c.Id, c.Description, c.WeightKg, c.VolumeM3, c.DeclaredValue, c.RiskCoefficient, c.CanBeStacked),
        nameof(DangerousCargo) => new DangerousCargo(c.Id, c.Description, c.WeightKg, c.VolumeM3, c.DeclaredValue, c.HazardClass),
        nameof(OversizedCargo) => new OversizedCargo(c.Id, c.Description, c.WeightKg, c.VolumeM3, c.DeclaredValue, c.LengthM, c.WidthM, c.HeightM),
        _ => throw new LogisticsSerializationException($"Неизвестный тип груза '{c.Kind}'.", new InvalidDataException())
    };
}

