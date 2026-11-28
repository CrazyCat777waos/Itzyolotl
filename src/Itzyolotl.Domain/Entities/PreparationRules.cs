namespace Itzyolotl.Domain.Entities;

internal static class PreparationRules
{
    public static void Validate(PreparationStation station, int minutes)
    {
        if (!Enum.IsDefined(station))
        {
            throw new ArgumentOutOfRangeException(nameof(station));
        }

        if (station == PreparationStation.None)
        {
            if (minutes != 0)
            {
                throw new ArgumentException(
                    "Un producto sin preparación debe tener 0 minutos.",
                    nameof(minutes));
            }

            return;
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minutes);
    }
}