using LogiCore.Domain.Infrastructure;

namespace LogiCore.Domain.Models;

public sealed class StandardCargo : Cargo
{
    public StandardCargo(Guid id, string description, decimal weightKg, decimal volumeM3, decimal declaredValue)
        : base(id, description, weightKg, volumeM3, declaredValue) { }
}


public sealed class PerishableCargo : Cargo, ITemperatureSensitive
{
    public PerishableCargo(Guid id, string description, decimal weightKg, decimal volumeM3, decimal declaredValue,
        DateTime expirationDate, decimal requiredTemperatureC)
        : base(id, description, weightKg, volumeM3, declaredValue)
    {
        if (expirationDate == default) throw new CargoValidationException("Дата окончания срока годности обязательна.");
        ExpirationDate = expirationDate;
        RequiredTemperatureC = requiredTemperatureC;
    }

    public DateTime ExpirationDate { get; }
    public decimal RequiredTemperatureC { get; }
}


public sealed class FragileCargo : Cargo, IStackable, IInsurable
{
    public FragileCargo(Guid id, string description, decimal weightKg, decimal volumeM3, decimal declaredValue,
        decimal riskCoefficient, bool canBeStacked = false)
        : base(id, description, weightKg, volumeM3, declaredValue)
    {
        if (riskCoefficient <= 0) throw new ArgumentOutOfRangeException(nameof(riskCoefficient));
        RiskCoefficient = riskCoefficient;
        _canBeStacked = canBeStacked;
    }

    public decimal RiskCoefficient { get; }
    private readonly bool _canBeStacked;
    
    bool IStackable.CanBeStacked => _canBeStacked;
    public decimal InsuranceRate => 0.015m;
}


public sealed class DangerousCargo : Cargo, IInsurable
{
    public DangerousCargo(Guid id, string description, decimal weightKg, decimal volumeM3, decimal declaredValue,
        int hazardClass)
        : base(id, description, weightKg, volumeM3, declaredValue)
    {
        if (hazardClass is < 1 or > 9) throw new CargoValidationException("Класс опасности должен быть от 1 до 9.");
        HazardClass = hazardClass;
    }

    public int HazardClass { get; }
    public decimal InsuranceRate => 0.03m;
}


public sealed class OversizedCargo : Cargo
{
    public OversizedCargo(Guid id, string description, decimal weightKg, decimal volumeM3, decimal declaredValue,
        decimal lengthM, decimal widthM, decimal heightM)
        : base(id, description, weightKg, volumeM3, declaredValue)
    {
        if (lengthM <= 0 || widthM <= 0 || heightM <= 0) throw new ArgumentOutOfRangeException("Габариты должны быть > 0.");
        LengthM = lengthM;
        WidthM = widthM;
        HeightM = heightM;
    }

    public decimal LengthM { get; }
    public decimal WidthM { get; }
    public decimal HeightM { get; }
}
