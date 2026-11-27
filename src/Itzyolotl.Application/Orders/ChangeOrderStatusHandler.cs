using Itzyolotl.Application.Abstractions;
using Itzyolotl.Contracts.Orders;
using Itzyolotl.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Itzyolotl.Application.Orders;

public sealed class ChangeOrderStatusHandler(
    IAppDbContext db,
    TimeProvider timeProvider,
    IOrderNotifier notifier)
{
    public async Task<ChangeOrderStatusResult> HandleAsync(
        Guid id,
        OrderActionRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.Action))
        {
            return ChangeOrderStatusResult.Invalid("La acción no es válida.");
        }

        string? reason = null;

        if (request.Action == OrderAction.Cancel)
        {
            reason = request.Reason?.Trim();

            if (string.IsNullOrEmpty(reason))
            {
                return ChangeOrderStatusResult.Invalid("Escribe el motivo de la cancelación.");
            }

            if (reason.Length > Order.MaxCancellationReasonLength)
            {
                return ChangeOrderStatusResult.Invalid(
                    $"El motivo no puede superar {Order.MaxCancellationReasonLength} caracteres.");
            }
        }

        var order = await db.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.IdempotencyKey == id, cancellationToken);

        if (order is null)
        {
            return ChangeOrderStatusResult.NotFound();
        }

        try
        {
            Apply(order, request.Action, reason, timeProvider.GetUtcNow());
        }
        catch (InvalidOperationException)
        {
            return ChangeOrderStatusResult.Conflict(
                "Esa acción ya no es posible: el pedido cambió de estado.");
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ChangeOrderStatusResult.Conflict(
                "Otro trabajador modificó este pedido al mismo tiempo. Revisa la cola.");
        }

        await notifier.NotifyQueueChangedAsync(cancellationToken);

        return ChangeOrderStatusResult.Ok(order.ToDto());
    }

    private static void Apply(Order order, OrderAction action, string? reason, DateTimeOffset now)
    {
        switch (action)
        {
            case OrderAction.MarkPaid:
                order.MarkPaid(now);
                break;
            case OrderAction.StartPreparation:
                order.StartPreparation(now);
                break;
            case OrderAction.MarkReady:
                order.MarkReady(now);
                break;
            case OrderAction.MarkDelivered:
                order.MarkDelivered(now);
                break;
            case OrderAction.Cancel:
                order.Cancel(reason ?? string.Empty, now);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }
}