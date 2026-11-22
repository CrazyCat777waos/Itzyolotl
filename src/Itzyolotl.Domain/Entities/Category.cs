using Itzyolotl.Domain.Common;

namespace Itzyolotl.Domain.Entities;

public class Category : Entity
{
    public string Name { get; private set; }
    public int DisplayOrder { get; private set; }

    public Category(string name, int displayOrder)
    {
        Name = NormalizeName(name);
        DisplayOrder = displayOrder;
    }

    public void Rename(string name) => Name = NormalizeName(name);

    private static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return name.Trim();
    }
}