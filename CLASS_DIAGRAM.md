# Упрощённая UML-структура LogiCore

```mermaid
classDiagram
    class Vehicle {
      <<abstract>>
      +Guid Id
      +decimal MaxLoadKg
      +decimal MaxVolumeM3
      +VehicleState State
      +CanCarry(Cargo) bool
      +CalculateDeliveryCost(Route, Cargo[]) decimal
    }
    Vehicle <|-- Truck
    Vehicle <|-- RefrigeratorTruck
    Vehicle <|-- CargoPlane
    Vehicle <|-- CargoShip
    Vehicle <|-- DroneCourier

    class Cargo {
      <<abstract>>
      +Guid Id
      +decimal WeightKg
      +decimal VolumeM3
      +decimal DeclaredValue
    }
    Cargo <|-- StandardCargo
    Cargo <|-- PerishableCargo
    Cargo <|-- FragileCargo
    Cargo <|-- DangerousCargo
    Cargo <|-- OversizedCargo

    class Order {
      +OrderStatus Status
      +Assign(Vehicle, decimal)
      +StartDelivery()
      +Complete()
      +Cancel()
    }
    Order --> Customer
    Order --> Cargo
    Order --> Route
    Order --> Vehicle

    class DeliveryService
    DeliveryService --> Vehicle
    DeliveryService --> Cargo
    DeliveryService --> Order
    DeliveryService --> CargoCompatibilityValidator
    DeliveryService --> ITariffStrategy

    class ITariffStrategy
      <<interface>>
    class IDeliveryCost
      <<interface>>
    class Repository~T~
      <<generic>>

    ITariffStrategy --> DeliveryService
    IDeliveryCost <|.. BaseDeliveryCost
```
