namespace LogiCore.Domain.Infrastructure;

public interface IEntity
{
    Guid Id { get; }
}

public interface IReadOnlyRepository<out T> where T : class, IEntity
{
    T? GetById(Guid id);
    IEnumerable<T> GetAll();
}

public interface IValidator<in T>
{
    ValidationResult Validate(T item);
}

public interface ITemperatureSensitive
{
    decimal RequiredTemperatureC { get; }
}

public interface IInsurable
{
    decimal InsuranceRate { get; }
}

public interface IStackable
{
    bool CanBeStacked { get; }
}


public sealed record ValidationResult(bool IsValid, string? ErrorMessage = null)
{
    public static ValidationResult Success() => new(true);
    public static ValidationResult Failure(string message) => new(false, message);
}
