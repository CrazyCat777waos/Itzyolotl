namespace Itzyolotl.Contracts.Orders;

public enum OrderStatusDto
{
    PendingPayment = 0,
    Paid = 1,
    InPreparation = 2,
    Ready = 3,
    Delivered = 4,
    Cancelled = 5
}

public sealed record OrderLineDto(
    int ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    string? Note,
    decimal Subtotal);

public sealed record OrderDto(
    Guid Id,
    string CustomerName,
    ServiceTypeDto ServiceType,
    OrderStatusDto Status,
    DateTimeOffset CreatedAt,
    decimal Total,
    IReadOnlyList<OrderLineDto> Lines);