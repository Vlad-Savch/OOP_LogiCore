using System;
using LogiCore.Domain.Models;
using LogiCore.Domain.Infrastructure;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace LogiCore.Domain.Patterns;



public abstract class VehicleCreator
{
    public abstract Vehicle Create(string registrationNumber);
}

public sealed class TruckCreator : VehicleCreator
{
    public override Vehicle Create(string registrationNumber) =>
        new Truck(Guid.NewGuid(), registrationNumber, 20000m, 82m, 75m, 2.5m);
}

public sealed class RefrigeratorTruckCreator : VehicleCreator
{
    public override Vehicle Create(string registrationNumber) =>
        new RefrigeratorTruck(Guid.NewGuid(), registrationNumber, 18000m, 72m, 70m, 3m, -20m, 8m);
}

public sealed class CargoPlaneCreator : VehicleCreator
{
    public override Vehicle Create(string registrationNumber) =>
        new CargoPlane(Guid.NewGuid(), registrationNumber, 50000m, 300m, 800m, 7m);
}

public sealed class CargoShipCreator : VehicleCreator
{
    public override Vehicle Create(string registrationNumber) =>
        new CargoShip(Guid.NewGuid(), registrationNumber, 200000m, 5000m, 35m, 0.8m);
}

public sealed class DroneCourierCreator : VehicleCreator
{
    public override Vehicle Create(string registrationNumber) =>
        new DroneCourier(Guid.NewGuid(), registrationNumber, 70m, 12m);
}

