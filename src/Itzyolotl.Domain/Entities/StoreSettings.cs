using Itzyolotl.Domain.Common;

namespace Itzyolotl.Domain.Entities;

public class StoreSettings : Entity
{
    public const int SingletonId = 1;
    public const int MinCapacity = 1;
    public const int MaxCapacity = 10;
    public const int DefaultCapacity = 1;

    public int CrepesCapacity { get; private set; }
    public int CoffeeAndPaninisCapacity { get; private set; }
    public int BackCapacity { get; private set; }

    public StoreSettings(int crepesCapacity, int coffeeAndPaninisCapacity, int backCapacity)
    {
        Id = SingletonId;
        CrepesCapacity = ValidateCapacity(crepesCapacity);
        CoffeeAndPaninisCapacity = ValidateCapacity(coffeeAndPaninisCapacity);
        BackCapacity = ValidateCapacity(backCapacity);
    }

    public int CapacityFor(PreparationStation station) => station switch
    {
        PreparationStation.Crepes => CrepesCapacity,
        PreparationStation.CoffeeAndPaninis => CoffeeAndPaninisCapacity,
        PreparationStation.Back => BackCapacity,
        _ => throw new ArgumentOutOfRangeException(nameof(station))
    };

    public void ChangeCapacity(PreparationStation station, int capacity)
    {
        var valid = ValidateCapacity(capacity);

        switch (station)
        {
            case PreparationStation.Crepes:
                CrepesCapacity = valid;
                break;
            case PreparationStation.CoffeeAndPaninis:
                CoffeeAndPaninisCapacity = valid;
                break;
            case PreparationStation.Back:
                BackCapacity = valid;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(station));
        }
    }

    private static int ValidateCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, MinCapacity);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(capacity, MaxCapacity);
        return capacity;
    }
}