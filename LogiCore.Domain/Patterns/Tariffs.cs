using System;
using LogiCore.Domain.Models;
using LogiCore.Domain.Infrastructure;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace LogiCore.Domain.Patterns;



public interface ITariffStrategy
{
    string Name { get; }
    decimal Calculate(decimal baseCost, Route route, IReadOnlyCollection<Cargo> cargo);
}



public sealed class StandardTariff : ITariffStrategy
{
    public string Name => "Standard";
    public decimal Calculate(decimal baseCost, Route route, IReadOnlyCollection<Cargo> cargo) => baseCost;
}

public sealed class ExpressTariff : ITariffStrategy
{
    public string Name => "Express";
    public decimal Calculate(decimal baseCost, Route route, IReadOnlyCollection<Cargo> cargo) => baseCost * 1.35m;
}

public sealed class HeavyCargoTariff : ITariffStrategy
{
    public string Name => "HeavyCargo";
    public decimal Calculate(decimal baseCost, Route route, IReadOnlyCollection<Cargo> cargo)
    {
        decimal weight = cargo.Sum(c => c.WeightKg);
        return baseCost * (weight > 5000m ? 1.25m : 1.05m);
    }
}



public sealed class TariffRegistry
{
    private static readonly Lazy<TariffRegistry> LazyInstance = new(() => new TariffRegistry());
    private readonly ConcurrentDictionary<string, ITariffStrategy> _strategies = new(StringComparer.OrdinalIgnoreCase);

    private TariffRegistry() { }
    public static TariffRegistry Instance => LazyInstance.Value;

    public void Register(ITariffStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        _strategies[strategy.Name] = strategy;
    }

    public ITariffStrategy Get(string name) =>
        _strategies.TryGetValue(name, out var strategy)
            ? strategy
            : throw new KeyNotFoundException($"Тариф '{name}' не зарегистрирован.");
}

