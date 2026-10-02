using LogiCore.Domain.Infrastructure;

namespace LogiCore.Domain.Models;

public sealed class Customer : IEntity
{
    private readonly List<Order> _orders = new();

    public Customer(Guid id, string name, string contact)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Имя клиента обязательно.", nameof(name)) : name;
        Contact = string.IsNullOrWhiteSpace(contact) ? throw new ArgumentException("Контакт клиента обязателен.", nameof(contact)) : contact;
    }

    public Guid Id { get; }
    public string Name { get; private set; }
    public string Contact { get; private set; }
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    internal void AddOrder(Order order) => _orders.Add(order);

    public override string ToString() => $"{Name} ({Contact})";
    public override bool Equals(object? obj) => obj is Customer other && Id == other.Id;
    public override int GetHashCode() => Id.GetHashCode();
}
