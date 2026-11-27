using Itzyolotl.Application.Abstractions;
using Itzyolotl.Contracts.Orders;
using Itzyolotl.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Itzyolotl.Application.Orders;

public sealed class CreateOrderHandler(IAppDbContext db, TimeProvider timeProvider, IOrderNotifier notifier)
{
    public const int MaxLines = OrderLimits.MaxLines;
    public const int MaxQuantityPerLine = OrderLimits.MaxQuantityPerLine;

    public async Task<CreateOrderResult> HandleAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var error = ValidateShape(request);

        if (error is not null)
        {
            return CreateOrderResult.Invalid(error);
        }

        var existing = await FindByKeyAsync(request.IdempotencyKey, cancellationToken);

        if (existing is not null)
        {
            return CreateOrderResult.AlreadyExists(existing.ToDto());
        }

        var productIds = request.Lines.Select(line => line.ProductId).Distinct().ToList();

        var products = await db.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        Order order;

        try
        {
            var lines = new List<OrderLine>();

            foreach (var line in request.Lines)
            {
                if (!products.TryGetValue(line.ProductId, out var product))
                {
                    return CreateOrderResult.Invalid("Uno de los productos ya no existe en el menú.");
                }

                if (!product.IsAvailable)
                {
                    return CreateOrderResult.Invalid($"«{product.Name}» no está disponible por el momento.");
                }

                lines.Add(new OrderLine(product.Id, product.Name, product.Price, line.Quantity, line.Note));
            }

            order = new Order(
                request.IdempotencyKey,
                request.CustomerName,
                request.ServiceType.ToDomain(),
                lines,
                timeProvider.GetUtcNow());
        }
        catch (ArgumentException)
        {
            return CreateOrderResult.Invalid("Los datos del pedido no son válidos.");
        }

        db.Orders.Add(order);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var duplicate = await FindByKeyAsync(request.IdempotencyKey, cancellationToken);

            if (duplicate is null)
            {
                throw;
            }

            return CreateOrderResult.AlreadyExists(duplicate.ToDto());
        }

        await notifier.NotifyQueueChangedAsync(cancellationToken);

        return CreateOrderResult.Created(order.ToDto());
    }

    private Task<Order?> FindByKeyAsync(Guid key, CancellationToken cancellationToken) =>
        db.Orders
            .AsNoTracking()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.IdempotencyKey == key, cancellationToken);

    private static string? ValidateShape(CreateOrderRequest request)
    {
        if (request.IdempotencyKey == Guid.Empty)
        {
            return "La clave del pedido no es válida.";
        }

        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            return "Escribe tu nombre para el pedido.";
        }

        if (request.CustomerName.Trim().Length > Order.MaxCustomerNameLength)
        {
            return $"El nombre no puede superar {Order.MaxCustomerNameLength} caracteres.";
        }

        if (!Enum.IsDefined(request.ServiceType))
        {
            return "Elige si comes aquí o es para llevar.";
        }

        if (request.Lines is null || request.Lines.Count == 0)
        {
            return "El pedido debe tener al menos un producto.";
        }

        if (request.Lines.Count > MaxLines)
        {
            return $"Un pedido no puede tener más de {MaxLines} productos distintos.";
        }

        foreach (var line in request.Lines)
        {
            if (line is null)
            {
                return "El pedido tiene un producto no válido.";
            }

            if (line.Quantity is < 1 or > MaxQuantityPerLine)
            {
                return $"La cantidad de cada producto debe estar entre 1 y {MaxQuantityPerLine}.";
            }

            if (line.Note is { } note && note.Trim().Length > OrderLine.MaxNoteLength)
            {
                return $"La nota no puede superar {OrderLine.MaxNoteLength} caracteres.";
            }
        }

        return null;
    }
}