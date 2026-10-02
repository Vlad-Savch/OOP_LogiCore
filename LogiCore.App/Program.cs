using System.Globalization;
using LogiCore.App;
using LogiCore.Domain.Models;
using LogiCore.Domain.Infrastructure;
using LogiCore.Domain.Patterns;
using LogiCore.Domain.Services;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var state = new LogisticsState();
DemoData.Populate(state);
var validator = new CargoCompatibilityValidator();
var tariffs = TariffRegistry.Instance;
tariffs.Register(new StandardTariff());
tariffs.Register(new ExpressTariff());
tariffs.Register(new HeavyCargoTariff());
var delivery = new DeliveryService(state, validator, tariffs);
var reports = new ReportService(state);
var persistence = new StatePersistenceService();
var notifier = new ConsoleNotifier();
notifier.Subscribe(delivery);
using var logger = new FileLogger("delivery.log");
logger.Subscribe(delivery);

RunDemo();
RunMenu();

void RunDemo()
{
    Console.WriteLine("=== LOGICORE ===");
    var cargoes = DemoData.CreateTenCargoes();
    Console.WriteLine($"Создан парк: {state.Vehicles.GetAll().Count()} ТС; грузов: {cargoes.Count}.");
    var truckRepository = new LogiCore.Domain.Infrastructure.Repository<Truck>();
    foreach (var truck in state.Vehicles.GetAll().OfType<Truck>()) truckRepository.Add(truck);
    LogiCore.Domain.Infrastructure.IReadOnlyRepository<Vehicle> covariantRepository = truckRepository;
    Console.WriteLine($"T3 covariance: {covariantRepository.GetAll().Count()} ТС получены через IReadOnlyRepository<Vehicle>.");

    var stackable = (LogiCore.Domain.Infrastructure.IStackable)cargoes[4];
    Console.WriteLine($"Explicit IStackable: CanBeStacked={stackable.CanBeStacked}");

    LogiCore.Domain.Infrastructure.IValidator<PerishableCargo> perishableValidator = validator;
    Console.WriteLine($"T3 contravariance: {perishableValidator.Validate((PerishableCargo)cargoes[2]).IsValid}");

    var customer = state.Customers.GetAll().First();
    var validOrder = new Order(Guid.NewGuid(), customer, new[] { cargoes[2], cargoes[4] }, DemoData.ShortRoute());
    delivery.AcceptOrder(validOrder);
    var vehicle = delivery.AssignCheapestVehicle(validOrder, tariffs.Get("Express"), insurance: true, urgent: true, fragilePackaging: true);
    Console.WriteLine($"Подобрано ТС: {vehicle}");
    Console.WriteLine($"Стоимость по шагам: {delivery.BuildDecoratedCost(validOrder, tariffs.Get("Express"), true, true, true).Describe()}");
    Console.WriteLine($"Оценка времени: {validOrder.Route.EstimateTime(vehicle)}");

    delivery.StartDelivery(validOrder);
    delivery.CompleteDelivery(validOrder);
    notifier.UnsubscribeStatusChanged(delivery);
    Console.WriteLine("ConsoleNotifier отписан от OrderStatusChanged; следующие изменения статуса в консоль не выводятся.");

    foreach (var item in new[] { cargoes[0], cargoes[1], cargoes[4], cargoes[8] })
    {
        var extra = new Order(Guid.NewGuid(), state.Customers.GetAll().Skip(1).First(), new[] { item }, DemoData.ShortRoute());
        delivery.AcceptOrder(extra);
        delivery.AssignCheapestVehicle(extra, tariffs.Get("Standard"), insurance: true);
        delivery.StartDelivery(extra);
        delivery.CompleteDelivery(extra);
    }

    try
    {
        var invalidOrder = new Order(Guid.NewGuid(), customer, new[] { cargoes[2], cargoes[6] }, DemoData.ShortRoute());
        delivery.AcceptOrder(invalidOrder);
    }
    catch (IncompatibleCargoException ex)
    {
        Console.WriteLine($"[HANDLED] IncompatibleCargoException: {ex.Message}");
    }

    try
    {
        var truck = state.Vehicles.GetAll().OfType<Truck>().First();
        var heavy = new StandardCargo(Guid.NewGuid(), "Сверхтяжёлый груз", 25000, 10, 1000000);
        delivery.AttemptOverload(truck, new[] { heavy });
    }
    catch (VehicleOverloadException ex) when (ex.Message.Contains("перегруз", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine($"[HANDLED when] VehicleOverloadException: {ex.Message}");
    }
    finally
    {
        Console.WriteLine("Проверка перегруза завершена (finally).");
    }

    PrintReports(reports);

    const string file = "logiCore-state.json";
    persistence.Save(file, state, delivery.Revenue);
    var before = state.Orders.GetAll().Select(o => (o.Id, o.Status, o.TotalCost)).OrderBy(x => x.Id).ToList();
    state.Clear();
    Console.WriteLine($"Состояние очищено: заказов={state.Orders.GetAll().Count()}, ТС={state.Vehicles.GetAll().Count()}.");
    var loaded = persistence.Load(file);
    state = loaded.State;
    logger.Unsubscribe(delivery);
    delivery = new DeliveryService(state, validator, tariffs, loaded.Revenue);
    reports = new ReportService(state);
    notifier.Subscribe(delivery);
    logger.Subscribe(delivery);
    var after = state.Orders.GetAll().Select(o => (o.Id, o.Status, o.TotalCost)).OrderBy(x => x.Id).ToList();
    Console.WriteLine($"Загрузка JSON: заказов={state.Orders.GetAll().Count()}, совпадение={before.SequenceEqual(after)}.");
}

void RunMenu()
{
    while (true)
    {
        Console.WriteLine("\n=== Меню ===");
        Console.WriteLine("1 - Показать отчёты");
        Console.WriteLine("2 - Сохранить состояние");
        Console.WriteLine("3 - Загрузить состояние");
        Console.WriteLine("4 - Создать демонстрационный заказ");
        Console.WriteLine("0 - Выход");
        Console.Write("Команда: ");
        string? command = Console.ReadLine();
        switch (command)
        {
            case "1": PrintReports(reports); break;
            case "2": persistence.Save("logiCore-state.json", state, delivery.Revenue); Console.WriteLine("Сохранено."); break;
            case "3":
                try
                {
                    var loaded = persistence.Load("logiCore-state.json");
                    logger.Unsubscribe(delivery);
                    state = loaded.State;
                    delivery = new DeliveryService(state, validator, tariffs, loaded.Revenue);
                    reports = new ReportService(state);
                    notifier.Subscribe(delivery);
                    logger.Subscribe(delivery);
                    Console.WriteLine("Загружено.");
                }
                catch (LogisticsException ex) when (ex is LogisticsSerializationException)
                {
                    Console.WriteLine($"Ошибка загрузки: {ex.Message}");
                }
                break;
            case "4": CreateDemoOrder(); break;
            case "0": return;
            default: Console.WriteLine("Неизвестная команда."); break;
        }
    }
}

void CreateDemoOrder()
{
    var cargo = new StandardCargo(Guid.NewGuid(), "Мебель", 1000, 10, 500000);
    var order = new Order(Guid.NewGuid(), state.Customers.GetAll().Last(), new[] { cargo }, DemoData.ShortRoute());
    try
    {
        delivery.AcceptOrder(order);
        var vehicle = delivery.AssignCheapestVehicle(order, tariffs.Get("Standard"), urgent: false, insurance: true);
        delivery.StartDelivery(order);
        delivery.CompleteDelivery(order);
        Console.WriteLine($"Создан и доставлен заказ на {vehicle.RegistrationNumber}, стоимость {order.TotalCost:0.00}.");
    }
    catch (LogisticsException ex)
    {
        Console.WriteLine($"Ошибка заказа: {ex.Message}");
    }
}

void PrintReports(ReportService reportService)
{
    Console.WriteLine("\n--- Отчёт 1: топ ТС по выручке ---");
    Console.WriteLine(reportService.TopVehiclesByRevenue().ToReportTable(x => x.VehicleType, x => x.Revenue.ToString("0.00")));
    Console.WriteLine("\n--- Отчёт 2: заказы по статусам ---");
    Console.WriteLine(reportService.OrdersByStatus().ToReportTable(x => x.Status, x => x.Count, x => x.Total.ToString("0.00")));
    Console.WriteLine("\n--- Отчёт 3: средняя загрузка ТС ---");
    Console.WriteLine(reportService.AverageVehicleLoadByType().ToReportTable(x => x.VehicleType, x => $"{x.AverageLoadPercent:0.00}%"));
    Console.WriteLine("\n--- Отчёт 4: клиенты выше порога ---");
    Console.WriteLine(reportService.CustomersAbove(1000m).ToReportTable(x => x.Customer, x => x.Total.ToString("0.00")));
    Console.WriteLine("\n--- Отчёт 5: груз - заказ - клиент ---");
    Console.WriteLine(reportService.CargoOrderCustomerJoin().ToReportTable(x => x.Cargo, x => x.Order, x => x.Customer));
    Console.WriteLine("\n--- Отчёт 6: класс опасности - количество ---");
    Console.WriteLine(reportService.DangerousClassCounts().ToReportTable(x => x.Key, x => x.Value));
    Console.WriteLine($"Средняя стоимость заказа: {reportService.AverageOrderValue():0.00}");
}
