using Itzyolotl.Contracts.Orders;
using Itzyolotl.Domain.Entities;

namespace Itzyolotl.Application.Orders;

internal static class OrderMapping
{
    public static OrderDto ToDto(this Order order) => new(
    order.IdempotencyKey,
    order.Id,
    order.CustomerName,
    order.ServiceType.ToDto(),
    order.Status.ToDto(),
    order.CreatedAt,
    order.PaidAt,
    order.Total,
    order.Lines
        .OrderBy(line => line.Id)
        .Select(line => new OrderLineDto(
            line.ProductId,
            line.ProductName,
            line.UnitPrice,
            line.Quantity,
            line.Note,
            line.Subtotal))
        .ToList());

    public static ServiceType ToDomain(this ServiceTypeDto serviceType) => serviceType switch
    {
        ServiceTypeDto.DineIn => ServiceType.DineIn,
        ServiceTypeDto.TakeAway => ServiceType.TakeAway,
        _ => throw new ArgumentOutOfRangeException(nameof(serviceType))
    };

    public static ServiceTypeDto ToDto(this ServiceType serviceType) => serviceType switch
    {
        ServiceType.DineIn => ServiceTypeDto.DineIn,
        ServiceType.TakeAway => ServiceTypeDto.TakeAway,
        _ => throw new ArgumentOutOfRangeException(nameof(serviceType))
    };

    public static OrderStatusDto ToDto(this OrderStatus status) => status switch
    {
        OrderStatus.PendingPayment => OrderStatusDto.PendingPayment,
        OrderStatus.Paid => OrderStatusDto.Paid,
        OrderStatus.InPreparation => OrderStatusDto.InPreparation,
        OrderStatus.Ready => OrderStatusDto.Ready,
        OrderStatus.Delivered => OrderStatusDto.Delivered,
        OrderStatus.Cancelled => OrderStatusDto.Cancelled,
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };
}