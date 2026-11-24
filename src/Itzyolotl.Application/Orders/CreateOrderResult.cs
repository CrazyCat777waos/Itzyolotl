using Itzyolotl.Contracts.Orders;

namespace Itzyolotl.Application.Orders;

public enum CreateOrderOutcome
{
    Created,
    AlreadyExists,
    Invalid
}

public sealed record CreateOrderResult(CreateOrderOutcome Outcome, OrderDto? Order, string? Error)
{
    public static CreateOrderResult Created(OrderDto order) =>
        new(CreateOrderOutcome.Created, order, null);

    public static CreateOrderResult AlreadyExists(OrderDto order) =>
        new(CreateOrderOutcome.AlreadyExists, order, null);

    public static CreateOrderResult Invalid(string error) =>
        new(CreateOrderOutcome.Invalid, null, error);
}