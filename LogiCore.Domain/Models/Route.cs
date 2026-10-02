using LogiCore.Domain.Infrastructure;

namespace LogiCore.Domain.Models;

public sealed class Route
{
    private readonly List<RoutePoint> _points;

    public Route(IEnumerable<RoutePoint> points)
    {
        _points = points?.ToList() ?? throw new ArgumentNullException(nameof(points));
        if (_points.Count < 2) throw new RouteNotFoundException("Маршрут должен содержать минимум две точки.");
        DistanceKm = CalculateDistance(_points);
    }

    public IReadOnlyList<RoutePoint> Points => _points.AsReadOnly();
    public decimal DistanceKm { get; }

    public TimeSpan EstimateTime(Vehicle vehicle)
    {
        if (vehicle is null) throw new ArgumentNullException(nameof(vehicle));
        if (vehicle.AverageSpeedKmH <= 0) throw new RouteNotFoundException("Скорость транспорта должна быть > 0.");
        return TimeSpan.FromHours((double)DistanceKm / (double)vehicle.AverageSpeedKmH);
    }

    private static decimal CalculateDistance(IReadOnlyList<RoutePoint> points)
    {
        double distance = 0;
        for (int i = 1; i < points.Count; i++) distance += points[i] - points[i - 1];
        return (decimal)distance;
    }
}
