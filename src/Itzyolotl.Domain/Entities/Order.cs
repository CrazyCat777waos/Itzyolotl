using Itzyolotl.Domain.Common;

namespace Itzyolotl.Domain.Entities;

public class Order : Entity
{
    public const int MaxCustomerNameLength = 60;
    public const int MaxCancellationReasonLength = 200;

    private readonly List<OrderLine> lines = [];

    public Guid IdempotencyKey { get; private set; }
    public string CustomerName { get; private set; } = null!;
    public ServiceType ServiceType { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.PendingPayment;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public DateTimeOffset? PreparationStartedAt { get; private set; }
    public DateTimeOffset? ReadyAt { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    public IReadOnlyList<OrderLine> Lines => lines;

    public decimal Total => lines.Sum(line => line.Subtotal);
    private Order()
    {
    }
    public Order(
        Guid idempotencyKey,
        string customerName,
        ServiceType serviceType,
        IEnumerable<OrderLine> orderLines,
        DateTimeOffset createdAt)
    {
        if (idempotencyKey == Guid.Empty)
        {
            throw new ArgumentException(
                "La clave de idempotencia no puede estar vacía.",
                nameof(idempotencyKey));
        }

        if (!Enum.IsDefined(serviceType))
        {
            throw new ArgumentOutOfRangeException(nameof(serviceType));
        }

        ArgumentNullException.ThrowIfNull(orderLines);
        lines.AddRange(orderLines);

        if (lines.Count == 0)
        {
            throw new ArgumentException(
                "El pedido debe tener al menos un producto.",
                nameof(orderLines));
        }

        IdempotencyKey = idempotencyKey;
        CustomerName = NormalizeCustomerName(customerName);
        ServiceType = serviceType;
        CreatedAt = createdAt;
    }

    public void MarkPaid(DateTimeOffset now)
    {
        EnsureStatus(OrderStatus.PendingPayment, "pagarse");
        Status = OrderStatus.Paid;
        PaidAt = now;
    }

    public void StartPreparation(DateTimeOffset now)
    {
        EnsureStatus(OrderStatus.Paid, "empezar a prepararse");
        Status = OrderStatus.InPreparation;
        PreparationStartedAt = now;
    }

    public void MarkReady(DateTimeOffset now)
    {
        EnsureStatus(OrderStatus.InPreparation, "marcarse como listo");
        Status = OrderStatus.Ready;
        ReadyAt = now;
    }

    public void MarkDelivered(DateTimeOffset now)
    {
        EnsureStatus(OrderStatus.Ready, "entregarse");
        Status = OrderStatus.Delivered;
        DeliveredAt = now;
    }

    public void Cancel(string reason, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var trimmedReason = reason.Trim();

        if (trimmedReason.Length > MaxCancellationReasonLength)
        {
            throw new ArgumentException(
                $"El motivo no puede superar {MaxCancellationReasonLength} caracteres.",
                nameof(reason));
        }

        if (Status is not (OrderStatus.PendingPayment or OrderStatus.Paid))
        {
            throw new InvalidOperationException(
                $"El pedido no puede cancelarse desde el estado {Status}.");
        }

        Status = OrderStatus.Cancelled;
        CancelledAt = now;
        CancellationReason = trimmedReason;
    }

    private void EnsureStatus(OrderStatus expected, string action)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException(
                $"El pedido no puede {action} desde el estado {Status}.");
        }
    }

    private static string NormalizeCustomerName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var trimmed = name.Trim();

        if (trimmed.Length > MaxCustomerNameLength)
        {
            throw new ArgumentException(
                $"El nombre no puede superar {MaxCustomerNameLength} caracteres.",
                nameof(name));
        }

        return trimmed;
    }
}