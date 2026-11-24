using Itzyolotl.Application.Abstractions;
using Itzyolotl.Contracts.Orders;

using Microsoft.EntityFrameworkCore;

namespace Itzyolotl.Application.Orders;

public sealed class GetOrderHandler(IAppDbContext db)
{
    public async Task<OrderDto?> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.IdempotencyKey == id, cancellationToken);

        return order?.ToDto();
    }
}