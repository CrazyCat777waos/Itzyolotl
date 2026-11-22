using Itzyolotl.Domain.Common;

namespace Itzyolotl.Domain.Entities;

public class Product : Entity
{
    public int CategoryId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int PreparationMinutes { get; private set; }
    public bool IsAvailable { get; private set; } = true;

    public Product(
        int categoryId,
        string name,
        decimal price,
        int preparationMinutes,
        string? description = null)
    {
        CategoryId = categoryId;
        Name = NormalizeName(name);
        Price = ValidatePrice(price);
        PreparationMinutes = ValidatePreparationMinutes(preparationMinutes);
        Description = description?.Trim();
    }

    public void ChangePrice(decimal price) => Price = ValidatePrice(price);

    public void SetAvailability(bool isAvailable) => IsAvailable = isAvailable;

    private static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return name.Trim();
    }

    private static decimal ValidatePrice(decimal price)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);
        return price;
    }

    private static int ValidatePreparationMinutes(int minutes)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minutes);
        return minutes;
    }
}