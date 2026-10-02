using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace LogiCore.Domain.Infrastructure;


public class LogisticsException : Exception
{
    public LogisticsException(string message) : base(message) { }
    public LogisticsException(string message, Exception innerException) : base(message, innerException) { }
}

public sealed class CargoValidationException : LogisticsException
{
    public CargoValidationException(string message) : base(message) { }
}

public sealed class IncompatibleCargoException : LogisticsException
{
    public IncompatibleCargoException(string message) : base(message) { }
}

public sealed class VehicleOverloadException : LogisticsException
{
    public VehicleOverloadException(string message) : base(message) { }
}

public sealed class RouteNotFoundException : LogisticsException
{
    public RouteNotFoundException(string message) : base(message) { }
}

public sealed class InvalidOrderStateException : LogisticsException
{
    public InvalidOrderStateException(string message) : base(message) { }
}

public sealed class LogisticsSerializationException : LogisticsException
{
    public LogisticsSerializationException(string message, Exception innerException) : base(message, innerException) { }
}

