using System.Globalization;

namespace LogiCore.Domain.Models;

public readonly struct RoutePoint
{
    public RoutePoint(double latitude, double longitude, string name)
    {
        if (latitude is < -90 or > 90) throw new ArgumentOutOfRangeException(nameof(latitude));
        if (longitude is < -180 or > 180) throw new ArgumentOutOfRangeException(nameof(longitude));
        Latitude = latitude;
        Longitude = longitude;
        Name = string.IsNullOrWhiteSpace(name) ? "Без названия" : name;
    }

    public double Latitude { get; }
    public double Longitude { get; }
    public string Name { get; }

    public static double operator -(RoutePoint a, RoutePoint b)
    {
        double latitude = a.Latitude - b.Latitude;
        double longitude = a.Longitude - b.Longitude;
        return Math.Sqrt(latitude * latitude + longitude * longitude) * 111;
    }

    public static explicit operator string(RoutePoint p) => p.ToString();

    public override string ToString() =>
        $"{Name} ({Latitude.ToString("0.####", CultureInfo.InvariantCulture)}, {Longitude.ToString("0.####", CultureInfo.InvariantCulture)})";
}
