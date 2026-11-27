using Itzyolotl.Application.Abstractions;
using Itzyolotl.Contracts.Orders;
using Itzyolotl.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Itzyolotl.Application.Orders;

public sealed class GetStaffQueueHandler(IAppDbContext db)
{
    public async Task<IReadOnlyList<OrderDto>> HandleAsync(CancellationToken cancellationToken)
    {
        // Pedidos activos = los anteriores a Delivered (los valores del enum son fijos a proposito).
        var orders = await db.Orders
            .AsNoTracking()
            .Include(o => o.Lines)
            .Where(o => o.Status < OrderStatus.Delivered)
            .OrderBy(o => o.PaidAt ?? o.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(order => order.ToDto()).ToList();
    }
}