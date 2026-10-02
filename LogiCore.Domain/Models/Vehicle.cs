using LogiCore.Domain.Infrastructure;

namespace LogiCore.Domain.Models;

public abstract class Vehicle : IEntity
{
    private static readonly Regex RegistrationPattern = new(@"^[A-ZА-Я0-9-]{4,12}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    protected Vehicle(Guid id, string registrationNumber, decimal maxLoadKg, decimal maxVolumeM3,
        decimal averageSpeedKmH, decimal baseRatePerKm)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        RegistrationNumber = registrationNumber;
        if (maxLoadKg <= 0 || maxVolumeM3 <= 0 || averageSpeedKmH <= 0 || baseRatePerKm < 0)
            throw new ArgumentOutOfRangeException(nameof(maxLoadKg), "Характеристики транспорта заданы неверно.");
        MaxLoadKg = maxLoadKg;
        MaxVolumeM3 = maxVolumeM3;
        AverageSpeedKmH = averageSpeedKmH;
        BaseRatePerKm = baseRatePerKm;
        State = VehicleState.Free;
    }

    public Guid Id { get; }
    public string RegistrationNumber
    {
        get => _registrationNumber;
        private set
        {
            if (!RegistrationPattern.IsMatch(value ?? string.Empty))
                throw new ArgumentException("Некорректный формат регистрационного номера.", nameof(value));
            _registrationNumber = value.Trim().ToUpperInvariant();
        }
    }
    private string _registrationNumber = string.Empty;
    public decimal MaxLoadKg { get; }
    public decimal MaxVolumeM3 { get; }
    public decimal AverageSpeedKmH { get; }
    public decimal BaseRatePerKm { get; }
    public VehicleState State { get; private set; }
    public virtual decimal MaxRangeKm => decimal.MaxValue;
    public virtual bool SupportsTemperatureControl => false;

    public abstract decimal CalculateDeliveryCost(Route route, IReadOnlyCollection<Cargo> cargo);

    public virtual bool CanCarry(Cargo cargo) =>
        cargo is not null &&
        cargo.WeightKg <= MaxLoadKg &&
        cargo.VolumeM3 <= MaxVolumeM3 &&
        (cargo is not LogiCore.Domain.Infrastructure.ITemperatureSensitive || SupportsTemperatureControl);

    public void MarkInTransit()
    {
        if (State != VehicleState.Free) throw new InvalidOperationException("ТС не свободно.");
        State = VehicleState.InTransit;
    }

    public void MarkFree()
    {
        State = VehicleState.Free;
    }

    public void MarkUnderMaintenance() => State = VehicleState.UnderMaintenance;

    public override string ToString() => $"{GetType().Name} {RegistrationNumber} ({State}, {MaxLoadKg:0} кг)";
    public override bool Equals(object? obj) => obj is Vehicle other && Id == other.Id;
    public override int GetHashCode() => Id.GetHashCode();
}
