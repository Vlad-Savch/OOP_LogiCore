using LogiCore.Domain.Infrastructure;

namespace LogiCore.Domain.Models;

public sealed class Order : IEntity
{
    private readonly List<Cargo> _cargo;

    public Order(Guid id, Customer customer, IEnumerable<Cargo> cargo, Route route)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Customer = customer ?? throw new ArgumentNullException(nameof(customer));
        _cargo = cargo?.ToList() ?? throw new ArgumentNullException(nameof(cargo));
        if (_cargo.Count == 0) throw new CargoValidationException("Заказ должен содержать хотя бы один груз.");
        Route = route ?? throw new ArgumentNullException(nameof(route));
        Status = OrderStatus.Created;
        customer.AddOrder(this);
    }

    public Guid Id { get; }
    public Customer Customer { get; }
    public IReadOnlyCollection<Cargo> Cargo => _cargo.AsReadOnly();
    public Route Route { get; }
    public Vehicle? AssignedVehicle { get; private set; }
    public decimal TotalCost { get; private set; }
    public OrderStatus Status { get; private set; }

    public void Assign(Vehicle vehicle, decimal totalCost)
    {
        EnsureStatus(OrderStatus.Created);
        AssignedVehicle = vehicle ?? throw new ArgumentNullException(nameof(vehicle));
        TotalCost = totalCost;
        Status = OrderStatus.Assigned;
    }

    public void StartDelivery()
    {
        EnsureStatus(OrderStatus.Assigned);
        Status = OrderStatus.InTransit;
    }

    public void Complete()
    {
        EnsureStatus(OrderStatus.InTransit);
        Status = OrderStatus.Delivered;
    }

    public void Cancel()
    {
        if (Status is not (OrderStatus.Created or OrderStatus.Assigned))
            throw new InvalidOrderStateException($"Заказ {Id} нельзя отменить из состояния {Status}.");
        Status = OrderStatus.Cancelled;
    }

    internal void RestoreState(OrderStatus status, Vehicle? vehicle, decimal totalCost)
    {
        AssignedVehicle = vehicle;
        TotalCost = totalCost;
        Status = status;
    }

    private void EnsureStatus(OrderStatus expected)
    {
        if (Status != expected)
            throw new InvalidOrderStateException($"Для заказа {Id} ожидается состояние {expected}, текущее: {Status}.");
    }

    public override string ToString() => $"Заказ {Id.ToString()[..8]}: {Customer.Name}, {Status}, {TotalCost:C}";
    public override bool Equals(object? obj) => obj is Order other && Id == other.Id;
    public override int GetHashCode() => Id.GetHashCode();
}
