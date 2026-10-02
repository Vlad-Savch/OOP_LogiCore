using LogiCore.Domain.Infrastructure;

namespace LogiCore.Domain.Models;

public abstract class Cargo : IEntity
{
    protected Cargo(Guid id, string description, decimal weightKg, decimal volumeM3, decimal declaredValue)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Description = string.IsNullOrWhiteSpace(description) ? throw new CargoValidationException("Описание груза не может быть пустым.") : description;
        WeightKg = ValidatePositive(weightKg, nameof(weightKg));
        VolumeM3 = ValidatePositive(volumeM3, nameof(volumeM3));
        if (declaredValue < 0) throw new CargoValidationException("DeclaredValue не может быть отрицательной.");
        DeclaredValue = declaredValue;
    }

    public Guid Id { get; }
    public string Description { get; }
    public decimal WeightKg { get; }
    public decimal VolumeM3 { get; }
    public decimal DeclaredValue { get; }

    private static decimal ValidatePositive(decimal value, string name) =>
        value > 0 ? value : throw new CargoValidationException($"{name} должен быть строго больше нуля.");

    public override string ToString() => $"{GetType().Name}: {Description}, {WeightKg:0.##} кг, {VolumeM3:0.##} м³";
    public override bool Equals(object? obj) => obj is Cargo other && Id == other.Id;
    public override int GetHashCode() => Id.GetHashCode();
}
