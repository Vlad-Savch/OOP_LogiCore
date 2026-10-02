using System;
using LogiCore.Domain.Models;
using LogiCore.Domain.Infrastructure;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace LogiCore.Domain.Patterns;



public interface IDeliveryCost
{
    decimal Total { get; }
    string Describe();
}

public sealed class BaseDeliveryCost : IDeliveryCost
{
    public BaseDeliveryCost(decimal total, string description)
    {
        Total = total;
        Description = description;
    }

    public decimal Total { get; }
    public string Description { get; }
    public string Describe() => $"База: {Total:0.00} ({Description})";
}

public abstract class DeliveryCostDecorator : IDeliveryCost
{
    protected DeliveryCostDecorator(IDeliveryCost inner) => Inner = inner ?? throw new ArgumentNullException(nameof(inner));
    protected IDeliveryCost Inner { get; }
    public abstract decimal Total { get; }
    public abstract string Describe();
}

public sealed class InsuranceDecorator : DeliveryCostDecorator
{
    public InsuranceDecorator(IDeliveryCost inner, IReadOnlyCollection<Cargo> cargo) : base(inner)
        => _cargo = cargo;
    private readonly IReadOnlyCollection<Cargo> _cargo;
    public override decimal Total => Inner.Total + _cargo.Sum(c => c.DeclaredValue) * 0.01m;
    public override string Describe() => $"{Inner.Describe()} -> +страхование = {Total:0.00}";
}

public sealed class UrgencyDecorator : DeliveryCostDecorator
{
    public UrgencyDecorator(IDeliveryCost inner, decimal coefficient = 1.20m) : base(inner) => _coefficient = coefficient;
    private readonly decimal _coefficient;
    public override decimal Total => Inner.Total * _coefficient;
    public override string Describe() => $"{Inner.Describe()} -> ×срочность {_coefficient:0.##} = {Total:0.00}";
}

public sealed class FragilePackagingDecorator : DeliveryCostDecorator
{
    public FragilePackagingDecorator(IDeliveryCost inner, decimal fixedCost = 350m) : base(inner) => _fixedCost = fixedCost;
    private readonly decimal _fixedCost;
    public override decimal Total => Inner.Total + _fixedCost;
    public override string Describe() => $"{Inner.Describe()} -> +упаковка = {Total:0.00}";
}

