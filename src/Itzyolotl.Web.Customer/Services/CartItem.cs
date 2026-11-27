namespace Itzyolotl.Web.Customer.Services;

public sealed record CartItem(
    int ProductId,
    string Name,
    decimal UnitPrice,
    int Quantity,
    string? Note)
{
    public decimal Subtotal => UnitPrice * Quantity;
}