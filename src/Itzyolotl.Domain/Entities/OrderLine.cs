using Itzyolotl.Domain.Common;

namespace Itzyolotl.Domain.Entities;

public class OrderLine : Entity
{
    public const int MaxNoteLength = 200;

    public int ProductId { get; private set; }
    public string ProductName { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public string? Note { get; private set; }

    public decimal Subtotal => UnitPrice * Quantity;

    public OrderLine(
        int productId,
        string productName,
        decimal unitPrice,
        int quantity,
        string? note = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(productId);
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(unitPrice);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        ProductId = productId;
        ProductName = productName.Trim();
        UnitPrice = unitPrice;
        Quantity = quantity;
        Note = NormalizeNote(note);
    }

    private static string? NormalizeNote(string? note)
    {
        var trimmed = note?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        if (trimmed.Length > MaxNoteLength)
        {
            throw new ArgumentException(
                $"La nota no puede superar {MaxNoteLength} caracteres.",
                nameof(note));
        }

        return trimmed;
    }
}