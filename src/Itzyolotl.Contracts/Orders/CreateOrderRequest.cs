namespace Itzyolotl.Contracts.Orders;

public enum ServiceTypeDto
{
    DineIn = 0,
    TakeAway = 1
}

public sealed record CreateOrderLineRequest(int ProductId, int Quantity, string? Note);

public sealed record CreateOrderRequest(
    Guid IdempotencyKey,
    string CustomerName,
    ServiceTypeDto ServiceType,
    IReadOnlyList<CreateOrderLineRequest> Lines);