using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LogiCore.Domain.Models;
using LogiCore.Domain.Infrastructure;
using LogiCore.Domain.Patterns;
namespace LogiCore.App;



public static class DemoData
{
    public static void Populate(LogisticsState state)
    {
        var creators = new VehicleCreator[]
        {
            new TruckCreator(), new TruckCreator(), new RefrigeratorTruckCreator(),
            new CargoPlaneCreator(), new CargoShipCreator(), new DroneCourierCreator()
        };
        string[] registrations = ["TRK-101", "TRK-102", "REF-201", "PLN-301", "SHP-401", "DRN-501"];
        for (int i = 0; i < creators.Length; i++) state.Vehicles.Add(creators[i].Create(registrations[i]));

        var customer1 = new Customer(Guid.NewGuid(), "ООО Альфа", "+7-900-000-01-01");
        var customer2 = new Customer(Guid.NewGuid(), "ИП Бета", "+7-900-000-02-02");
        var customer3 = new Customer(Guid.NewGuid(), "ООО Гамма", "+7-900-000-03-03");
        state.Customers.Add(customer1); state.Customers.Add(customer2); state.Customers.Add(customer3);
    }

    public static Route ShortRoute() => new([
        new RoutePoint(55.7558, 37.6173, "Москва"),
        new RoutePoint(55.8000, 37.7000, "Север"),
        new RoutePoint(55.9000, 37.8000, "Склад")
    ]);

    public static Route LongRoute() => new([
        new RoutePoint(55.7558, 37.6173, "Москва"),
        new RoutePoint(56.3269, 44.0059, "Нижний Новгород"),
        new RoutePoint(55.0415, 82.9346, "Новосибирск")
    ]);

    public static List<Cargo> CreateTenCargoes()
    {
        DateTime expiry = DateTime.UtcNow.AddDays(10);
        return [
            new StandardCargo(Guid.NewGuid(), "Электроника", 500, 4, 800000),
            new StandardCargo(Guid.NewGuid(), "Текстиль", 800, 8, 200000),
            new PerishableCargo(Guid.NewGuid(), "Фрукты", 1200, 10, 150000, expiry, 4),
            new PerishableCargo(Guid.NewGuid(), "Молочная продукция", 700, 6, 110000, expiry, 2),
            new FragileCargo(Guid.NewGuid(), "Стекло", 400, 5, 300000, 1.5m),
            new FragileCargo(Guid.NewGuid(), "Медицинское оборудование", 600, 7, 900000, 1.3m),
            new DangerousCargo(Guid.NewGuid(), "Растворитель", 900, 8, 120000, 3),
            new DangerousCargo(Guid.NewGuid(), "Химреактив", 500, 4, 180000, 6),
            new OversizedCargo(Guid.NewGuid(), "Промышленный станок", 7000, 60, 2500000, 6, 2.4m, 2.8m),
            new StandardCargo(Guid.NewGuid(), "Запчасти", 1500, 12, 400000)
        ];
    }
}

