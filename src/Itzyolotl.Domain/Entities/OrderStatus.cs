namespace Itzyolotl.Domain.Entities;

public enum OrderStatus
{
    PendingPayment = 0,
    Paid = 1,
    InPreparation = 2,
    Ready = 3,
    Delivered = 4,
    Cancelled = 5
}