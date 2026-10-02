using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LogiCore.Domain.Models;
namespace LogiCore.Domain.Infrastructure;



public class Repository<T> : IReadOnlyRepository<T>, IEnumerable<T> where T : class, IEntity
{
    private readonly Dictionary<Guid, T> _items = new();

    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!_items.TryAdd(item.Id, item))
            throw new InvalidOperationException($"Объект с Id {item.Id} уже существует.");
    }

    public bool Remove(Guid id) => _items.Remove(id);

    public IEnumerable<T> FindAll(Predicate<T> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        foreach (T item in _items.Values)
            if (predicate(item)) yield return item;
    }

    public T? GetById(Guid id) => _items.TryGetValue(id, out T? item) ? item : null;

    public IEnumerable<T> GetAll() => this;

    public T? this[Guid id] => GetById(id);
    
    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in _items.Values) yield return item;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}



public sealed class LogisticsState
{
    public Repository<Vehicle> Vehicles { get; } = new();
    public Repository<Customer> Customers { get; } = new();
    public Repository<Order> Orders { get; } = new();

    public void Clear()
    {
        foreach (var order in Orders.GetAll().ToList()) Orders.Remove(order.Id);
        foreach (var customer in Customers.GetAll().ToList()) Customers.Remove(customer.Id);
        foreach (var vehicle in Vehicles.GetAll().ToList()) Vehicles.Remove(vehicle.Id);
    }
}

