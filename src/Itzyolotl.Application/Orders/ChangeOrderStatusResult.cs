using Itzyolotl.Contracts.Orders;

namespace Itzyolotl.Application.Orders;

public enum ChangeOrderOutcome
{
    Ok,
    NotFound,
    Invalid,
    Conflict
}

public sealed record ChangeOrderStatusResult(
    ChangeOrderOutcome Outcome,
    OrderDto? Order,
    string? Error)
{
    public static ChangeOrderStatusResult Ok(OrderDto order) =>
        new(ChangeOrderOutcome.Ok, order, null);

    public static ChangeOrderStatusResult NotFound() =>
        new(ChangeOrderOutcome.NotFound, null, null);

    public static ChangeOrderStatusResult Invalid(string error) =>
        new(ChangeOrderOutcome.Invalid, null, error);

    public static ChangeOrderStatusResult Conflict(string error) =>
        new(ChangeOrderOutcome.Conflict, null, error);
}