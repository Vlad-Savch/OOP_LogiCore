# LogiCore — учебный проект по ООП C#

## Структура

- **LogiCore.Domain** — доменная модель, сервисы и паттерны.
- **LogiCore.App** — демо-сценарий, уведомления, интерактивное меню.
- **LogiCore.Tests** — xUnit-тесты.

### Domain

```text
Vehicle.cs              базовый Vehicle
VehicleTypes.cs         Truck, RefrigeratorTruck, CargoPlane, CargoShip, DroneCourier
Cargo.cs                базовый Cargo
CargoTypes.cs           StandardCargo, PerishableCargo, FragileCargo, DangerousCargo, OversizedCargo
Customer.cs             клиент и история заказов
RoutePoint.cs           RoutePoint и перегрузка -
Route.cs                маршрут и расчёт расстояния и времени
Order.cs                заказ и машина состояний
Contracts.cs            интерфейсы
Types.cs                enum и [Flags]
Exceptions.cs           иерархия исключений
Events.cs               события и EventArgs
Dto.cs                  DTO для JSON-снимка
Repository.cs           Generic-хранилище и состояние системы(Repository<T> + LogisticsState)
Extensions.cs           Extension method для отчётов
Tariffs.cs              Strategy + Singleton
Costs.cs                Decorator
Factories.cs            Factory Method
Compatibility.cs        проверка совместимости грузов
DeliveryService.cs      главный диспетчер системы
Reports.cs              LINQ-отчёты
Persistence.cs          JSON сохранение/загрузка
```

## Паттерны

1. **Strategy** — `ITariffStrategy` и тарифные стратегии.
2. **Decorator** — дополнительные услуги поверх стоимости.
3. **Factory Method** — создание разных типов транспорта.
4. **Observer** — события `DeliveryService`.
5. **Singleton** — реестр тарифов через `Lazy<T>`.

## T1–T10

| Требование                           | Где сделано                                                                                                  |
|--------------------------------------|--------------------------------------------------------------------------------------------------------------|
| T1 Инкапсуляция                      | `Models`, конструкторы, private set, read-only коллекции                                                     |
| T2 Наследование и полиморфизм        | `Vehicle.cs`, `VehicleTypes.cs`, `Cargo.cs`, `CargoTypes.cs`                                                 |
| T3 Интерфейсы и вариантность         | `Contracts.cs`, covariance `IReadOnlyRepository<out T>`, contravariance `IValidator<in T>`, explicit `IStackable` |
| T4 Обобщения и собственная коллекция | `Repository.cs`, `FindAll(Predicate<T>)`, индексатор, `yield return`, extension                              |
| T5 Делегаты и события                | `Events.cs`, `DeliveryService.cs`, `Observers.cs`                                                            |
| T6 Исключения                        | `Exceptions.cs`, `when`, `throw;`, `using/finally`                                                           |
| T7 Паттерны                          | `Patterns/*`, `Observers.cs`                                                                                 |
| T8 Коллекции и LINQ                  | `Reports.cs` — 6 отчётов, query + method syntax                                                              |
| T9 Сериализация                      | `Persistence.cs`, `Dto.cs`                                                                                   |
| T10 Enum и структуры                 | `Types.cs`, `RoutePoint.cs`                                                                                  |

