using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LogiCore.Domain.Services;
using LogiCore.Domain.Infrastructure;
namespace LogiCore.App;



public sealed class ConsoleNotifier
{
    public void Subscribe(DeliveryService service)
    {
        service.OrderCreated += OnOrderCreated;
        service.OrderStatusChanged += OnOrderStatusChanged;
        service.VehicleOverloadAttempt += OnVehicleOverloadAttempt;
        service.DeliveryCompleted += OnDeliveryCompleted;
    }

    public void UnsubscribeStatusChanged(DeliveryService service) => service.OrderStatusChanged -= OnOrderStatusChanged;

    private static void OnOrderCreated(object sender, OrderCreatedEventArgs e) =>
        Console.WriteLine($"[EVENT] Создан заказ {e.Order.Id.ToString()[..8]}.");

    private static void OnOrderStatusChanged(object sender, OrderStatusChangedEventArgs e) =>
        Console.WriteLine($"[EVENT] Заказ {e.Order.Id.ToString()[..8]}: {e.OldStatus} -> {e.NewStatus}.");

    private static void OnVehicleOverloadAttempt(object sender, VehicleOverloadAttemptEventArgs e) =>
        Console.WriteLine($"[EVENT] Перегруз ТС {e.Vehicle.RegistrationNumber}: {e.WeightKg:0.##} кг, {e.VolumeM3:0.##} м³.");

    private static void OnDeliveryCompleted(object sender, DeliveryCompletedEventArgs e) =>
        Console.WriteLine($"[EVENT] Доставка завершена. Выручка: {e.Revenue:0.00}.");
}



public sealed class FileLogger : IDisposable
{
    private readonly string _path;
    private DeliveryService? _service;

    public FileLogger(string path) => _path = path;

    public void Subscribe(DeliveryService service)
    {
        if (_service is not null) Unsubscribe();
        _service = service;
        service.OrderCreated += OnOrderCreated;
        service.OrderStatusChanged += OnOrderStatusChanged;
        service.VehicleOverloadAttempt += OnVehicleOverloadAttempt;
        service.DeliveryCompleted += OnDeliveryCompleted;
    }

    public void Dispose() => Unsubscribe();

    public void Unsubscribe(DeliveryService service) => UnsubscribeCore(service);

    private void Unsubscribe()
    {
        if (_service is not null) UnsubscribeCore(_service);
    }

    private void UnsubscribeCore(DeliveryService service)
    {
        service.OrderCreated -= OnOrderCreated;
        service.OrderStatusChanged -= OnOrderStatusChanged;
        service.VehicleOverloadAttempt -= OnVehicleOverloadAttempt;
        service.DeliveryCompleted -= OnDeliveryCompleted;
    }

    private void Write(string text)
    {
        using var writer = new StreamWriter(_path, append: true);
        writer.WriteLine($"{DateTime.Now:O} {text}");
    }

    private void OnOrderCreated(object sender, OrderCreatedEventArgs e) => Write($"OrderCreated {e.Order.Id}");
    private void OnOrderStatusChanged(object sender, OrderStatusChangedEventArgs e) => Write($"OrderStatusChanged {e.Order.Id}: {e.OldStatus}->{e.NewStatus}");
    private void OnVehicleOverloadAttempt(object sender, VehicleOverloadAttemptEventArgs e) => Write($"VehicleOverloadAttempt {e.Vehicle.RegistrationNumber}: {e.WeightKg}kg");
    private void OnDeliveryCompleted(object sender, DeliveryCompletedEventArgs e) => Write($"DeliveryCompleted {e.Order.Id}: {e.Revenue}");
}

