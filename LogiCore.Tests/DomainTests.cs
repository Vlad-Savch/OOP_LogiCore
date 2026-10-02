using LogiCore.Domain.Models;
using LogiCore.Domain.Infrastructure;
using LogiCore.Domain.Patterns;
using LogiCore.Domain.Services;
using Xunit;

namespace LogiCore.Tests;

public class DomainTests
{
    private static Route Route(double offset = 0) => new([
        new RoutePoint(55.7558 + offset, 37.6173, "A"),
        new RoutePoint(55.8000 + offset, 37.7000, "B")
    ]);

    [Fact]
    public void Cargo_RejectsNonPositiveWeight() =>
        Assert.Throws<CargoValidationException>(() => new StandardCargo(Guid.NewGuid(), "x", 0, 1, 0));

    [Fact]
    public void RegistrationNumber_IsValidated() =>
        Assert.Throws<ArgumentException>(() => new Truck(Guid.NewGuid(), "bad space", 100, 10, 50, 1));

    [Fact]
    public void RoutePoint_SubtractionReturnsDistance() =>
        Assert.True(RoutePointDistance() > 0);

    private static double RoutePointDistance() =>
        new RoutePoint(55.7558, 37.6173, "A") - new RoutePoint(55.8000, 37.7000, "B");

    [Fact]
    public void Truck_Cost_IsCalculatedPolymorphically() =>
        Assert.True(new Truck(Guid.NewGuid(), "TRK-001", 1000, 10, 60, 2).CalculateDeliveryCost(Route(), Array.Empty<Cargo>()) > 0);

    [Fact]
    public void RefrigeratorTruck_Cost_IsCalculated() =>
        Assert.True(new RefrigeratorTruck(Guid.NewGuid(), "REF-001", 1000, 10, 60, 2, -20, 8).CalculateDeliveryCost(Route(), Array.Empty<Cargo>()) > 0);

    [Fact]
    public void Plane_Cost_IsCalculated() =>
        Assert.True(new CargoPlane(Guid.NewGuid(), "PLN-001", 1000, 10, 600, 3).CalculateDeliveryCost(Route(), Array.Empty<Cargo>()) > 0);

    [Fact]
    public void Ship_Cost_IsCalculated() =>
        Assert.True(new CargoShip(Guid.NewGuid(), "SHP-001", 1000, 10, 30, 1).CalculateDeliveryCost(Route(), Array.Empty<Cargo>()) > 0);

    [Fact]
    public void Drone_Cost_IsCalculated() =>
        Assert.True(new DroneCourier(Guid.NewGuid(), "DRN-001", 70, 5).CalculateDeliveryCost(Route(), Array.Empty<Cargo>()) > 0);

    [Fact]
    public void EveryVehicle_CalculatesZeroCostAtZeroDistance()
    {
        var zeroRoute = new Route([
            new RoutePoint(55.7558, 37.6173, "A"),
            new RoutePoint(55.7558, 37.6173, "A")
        ]);
        var cargoes = Array.Empty<Cargo>();
        Vehicle[] vehicles = [
            new Truck(Guid.NewGuid(), "TRK-020", 100, 10, 60, 2),
            new RefrigeratorTruck(Guid.NewGuid(), "REF-020", 100, 10, 60, 2, -20, 8),
            new CargoPlane(Guid.NewGuid(), "PLN-020", 100, 10, 600, 2),
            new CargoShip(Guid.NewGuid(), "SHP-020", 100, 10, 30, 2),
            new DroneCourier(Guid.NewGuid(), "DRN-020", 70, 5)
        ];
        foreach (var vehicle in vehicles)
            Assert.Equal(0m, vehicle.CalculateDeliveryCost(zeroRoute, cargoes));
    }

    [Fact]
    public void CompatibilityValidator_RejectsPerishableForNonRefrigeratedVehicle()
    {
        var validator = new CargoCompatibilityValidator();
        var vehicle = new Truck(Guid.NewGuid(), "TRK-021", 100, 10, 60, 2);
        var cargo = new PerishableCargo(Guid.NewGuid(), "food", 10, 1, 10, DateTime.UtcNow.AddDays(1), 4);
        Assert.Throws<IncompatibleCargoException>(() => validator.ValidateForVehicleOrThrow(vehicle, [cargo]));
    }

    [Fact]
    public void Vehicle_CanCarry_UsesBoundaryWeight()
    {
        var vehicle = new Truck(Guid.NewGuid(), "TRK-002", 100, 10, 60, 2);
        var cargo = new StandardCargo(Guid.NewGuid(), "x", 100, 10, 0);
        Assert.True(vehicle.CanCarry(cargo));
    }

    [Fact]
    public void Compatibility_RejectsDangerousAndPerishableTogether()
    {
        var validator = new CargoCompatibilityValidator();
        var perishable = new PerishableCargo(Guid.NewGuid(), "food", 10, 1, 10, DateTime.UtcNow.AddDays(1), 4);
        var dangerous = new DangerousCargo(Guid.NewGuid(), "chem", 10, 1, 10, 3);
        Assert.Throws<IncompatibleCargoException>(() => validator.ValidateOrThrow([perishable, dangerous]));
    }

    [Fact]
    public void Compatibility_RejectsExpiredCargo()
    {
        var validator = new CargoCompatibilityValidator();
        var expired = new PerishableCargo(Guid.NewGuid(), "food", 10, 1, 10, DateTime.UtcNow.AddDays(-1), 4);
        Assert.Throws<CargoValidationException>(() => validator.ValidateOrThrow([expired]));
    }

    [Fact]
    public void Compatibility_RejectsVehicleOverload()
    {
        var validator = new CargoCompatibilityValidator();
        var vehicle = new Truck(Guid.NewGuid(), "TRK-003", 100, 10, 60, 2);
        var cargo = new StandardCargo(Guid.NewGuid(), "x", 101, 1, 10);
        Assert.Throws<VehicleOverloadException>(() => validator.ValidateForVehicleOrThrow(vehicle, [cargo]));
    }

    [Fact]
    public void Refrigerator_RejectsWrongTemperature()
    {
        var vehicle = new RefrigeratorTruck(Guid.NewGuid(), "REF-002", 100, 10, 60, 2, -5, 5);
        var cargo = new PerishableCargo(Guid.NewGuid(), "ice", 10, 1, 10, DateTime.UtcNow.AddDays(1), -10);
        Assert.False(vehicle.CanCarry(cargo));
    }

    [Fact]
    public void Order_StateMachine_AllowsValidSequence()
    {
        var customer = new Customer(Guid.NewGuid(), "A", "C");
        var cargo = new StandardCargo(Guid.NewGuid(), "x", 10, 1, 10);
        var order = new Order(Guid.NewGuid(), customer, [cargo], Route());
        var vehicle = new Truck(Guid.NewGuid(), "TRK-004", 100, 10, 60, 2);
        order.Assign(vehicle, 100);
        order.StartDelivery();
        order.Complete();
        Assert.Equal(OrderStatus.Delivered, order.Status);
    }

    [Fact]
    public void Order_StateMachine_RejectsCompleteFromCreated()
    {
        var customer = new Customer(Guid.NewGuid(), "A", "C");
        var cargo = new StandardCargo(Guid.NewGuid(), "x", 10, 1, 10);
        var order = new Order(Guid.NewGuid(), customer, [cargo], Route());
        Assert.Throws<InvalidOrderStateException>(order.Complete);
    }

    [Fact]
    public void Repository_AddRemoveFindIteratorIndexer()
    {
        var repo = new Repository<Customer>();
        var a = new Customer(Guid.NewGuid(), "A", "1");
        var b = new Customer(Guid.NewGuid(), "B", "2");
        repo.Add(a); repo.Add(b);
        Assert.Same(a, repo[a.Id]);
        Assert.Single(repo.FindAll(x => x.Name == "A"));
        Assert.Equal(2, repo.ToList().Count);
        Assert.True(repo.Remove(b.Id));
        Assert.Null(repo.GetById(b.Id));
    }

    [Fact]
    public void Contravariance_AllowsGeneralCargoValidator()
    {
        var validator = new CargoCompatibilityValidator();
        LogiCore.Domain.Infrastructure.IValidator<PerishableCargo> specific = validator;
        var cargo = new PerishableCargo(Guid.NewGuid(), "food", 1, 1, 1, DateTime.UtcNow.AddDays(1), 4);
        Assert.True(specific.Validate(cargo).IsValid);
    }

    [Fact]
    public void Decorators_Compose_AndOrderChangesResult()
    {
        var baseCost = new BaseDeliveryCost(100m, "base");
        var first = new UrgencyDecorator(new InsuranceDecorator(baseCost, [new StandardCargo(Guid.NewGuid(), "x", 1, 1, 1000)]));
        var second = new InsuranceDecorator(new UrgencyDecorator(baseCost), [new StandardCargo(Guid.NewGuid(), "x", 1, 1, 1000)]);
        Assert.NotEqual(first.Total, second.Total);
    }

    [Fact]
    public void TariffStrategy_ChangesResultWithoutChangingVehicle()
    {
        var route = Route();
        var cargo = new StandardCargo(Guid.NewGuid(), "x", 1000, 2, 100);
        var truck = new Truck(Guid.NewGuid(), "TRK-005", 2000, 10, 60, 2);
        var baseCost = truck.CalculateDeliveryCost(route, [cargo]);
        Assert.True(new ExpressTariff().Calculate(baseCost, route, [cargo]) > new StandardTariff().Calculate(baseCost, route, [cargo]));
    }

    [Fact]
    public void Serialization_RoundTrip_PreservesState()
    {
        var state = new LogisticsState();
        var customer = new Customer(Guid.NewGuid(), "A", "C");
        state.Customers.Add(customer);
        var vehicle = new Truck(Guid.NewGuid(), "TRK-006", 1000, 10, 60, 2);
        state.Vehicles.Add(vehicle);
        var cargo = new StandardCargo(Guid.NewGuid(), "x", 100, 1, 1000);
        var order = new Order(Guid.NewGuid(), customer, [cargo], Route());
        order.Assign(vehicle, 250);
        state.Orders.Add(order);
        string path = Path.Combine(Path.GetTempPath(), $"logicore-{Guid.NewGuid()}.json");
        try
        {
            var storage = new StatePersistenceService();
            storage.Save(path, state, 250);
            var loaded = storage.Load(path);
            var loadedOrder = Assert.Single(loaded.State.Orders.GetAll());
            Assert.Equal(order.Id, loadedOrder.Id);
            Assert.Equal(order.TotalCost, loadedOrder.TotalCost);
            Assert.Equal(OrderStatus.Assigned, loadedOrder.Status);
            Assert.Equal(vehicle.RegistrationNumber, loadedOrder.AssignedVehicle!.RegistrationNumber);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void MissingJson_ReturnsEmptyState()
    {
        var loaded = new StatePersistenceService().Load(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json"));
        Assert.Empty(loaded.State.Orders.GetAll());
    }

    [Fact]
    public void DeliveryService_RaisesOverloadEvent()
    {
        var state = new LogisticsState();
        var delivery = new DeliveryService(state, new CargoCompatibilityValidator(), TariffRegistry.Instance);
        var vehicle = new Truck(Guid.NewGuid(), "TRK-007", 10, 1, 60, 2);
        var cargo = new StandardCargo(Guid.NewGuid(), "heavy", 11, 1, 10);
        state.Vehicles.Add(vehicle);
        bool raised = false;
        delivery.VehicleOverloadAttempt += (_, _) => raised = true;
        Assert.Throws<VehicleOverloadException>(() => delivery.AttemptOverload(vehicle, [cargo]));
        Assert.True(raised);
    }
}
