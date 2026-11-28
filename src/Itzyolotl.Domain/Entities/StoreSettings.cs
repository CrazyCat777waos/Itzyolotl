using Itzyolotl.Domain.Common;

namespace Itzyolotl.Domain.Entities;

public class StoreSettings : Entity
{
    public const int SingletonId = 1;
    public const int MinCapacity = 1;
    public const int MaxCapacity = 10;
    public const int DefaultCapacity = 2;

    public int Capacity { get; private set; }

    public StoreSettings(int capacity)
    {
        Id = SingletonId;
        Capacity = ValidateCapacity(capacity);
    }

    public void ChangeCapacity(int capacity) => Capacity = ValidateCapacity(capacity);

    private static int ValidateCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, MinCapacity);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(capacity, MaxCapacity);
        return capacity;
    }
}