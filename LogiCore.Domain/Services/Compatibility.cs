using System;
using LogiCore.Domain.Models;
using LogiCore.Domain.Infrastructure;
using LogiCore.Domain.Patterns;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace LogiCore.Domain.Services;



public sealed class CargoCompatibilityValidator : IValidator<Cargo>, IValidator<IReadOnlyCollection<Cargo>>
{
    public ValidationResult Validate(Cargo item) => ValidationResult.Success();

    public ValidationResult Validate(IReadOnlyCollection<Cargo> cargo)
    {
        if (cargo.Count == 0) return ValidationResult.Failure("Набор грузов пуст.");
        if (cargo.OfType<PerishableCargo>().Any(p => p.ExpirationDate < DateTime.UtcNow))
            return ValidationResult.Failure("Есть груз с истёкшим сроком годности.");
        if (cargo.OfType<DangerousCargo>().Any() && cargo.OfType<PerishableCargo>().Any())
            return ValidationResult.Failure("Опасные грузы нельзя перевозить вместе со скоропортящимися.");
        return ValidationResult.Success();
    }

    public void ValidateOrThrow(IReadOnlyCollection<Cargo> cargo)
    {
        var result = Validate(cargo);
        if (!result.IsValid)
        {
            if (cargo.OfType<DangerousCargo>().Any() && cargo.OfType<PerishableCargo>().Any())
                throw new IncompatibleCargoException(result.ErrorMessage!);
            throw new CargoValidationException(result.ErrorMessage!);
        }
    }

    public void ValidateForVehicleOrThrow(Vehicle vehicle, IReadOnlyCollection<Cargo> cargo)
    {
        ValidateOrThrow(cargo);
        decimal weight = cargo.Sum(c => c.WeightKg);
        decimal volume = cargo.Sum(c => c.VolumeM3);
        if (weight > vehicle.MaxLoadKg || volume > vehicle.MaxVolumeM3)
            throw new VehicleOverloadException($"ТС {vehicle.RegistrationNumber} перегружено: {weight:0.##}/{vehicle.MaxLoadKg:0.##} кг, {volume:0.##}/{vehicle.MaxVolumeM3:0.##} м³.");
        foreach (var item in cargo)
        {
            if (!vehicle.CanCarry(item))
                throw new IncompatibleCargoException($"{vehicle.GetType().Name} не подходит для груза '{item.Description}'.");
        }
    }
}

