namespace Itzyolotl.Contracts.Orders;

public enum OrderAction
{
    MarkPaid = 0,
    StartPreparation = 1,
    MarkReady = 2,
    MarkDelivered = 3,
    Cancel = 4
}

public sealed record OrderActionRequest(OrderAction Action, string? Reason);