using Itzyolotl.Domain.Common;

namespace Itzyolotl.Domain.Entities;

public class OpeningPeriod : Entity
{
    public DayOfWeek Day { get; private set; }
    public TimeOnly OpensAt { get; private set; }
    public TimeOnly ClosesAt { get; private set; }

    public OpeningPeriod(DayOfWeek day, TimeOnly opensAt, TimeOnly closesAt)
    {
        if (!Enum.IsDefined(day))
        {
            throw new ArgumentOutOfRangeException(nameof(day));
        }

        if (closesAt <= opensAt)
        {
            throw new ArgumentException(
                "La hora de cierre debe ser posterior a la de apertura.",
                nameof(closesAt));
        }

        Day = day;
        OpensAt = opensAt;
        ClosesAt = closesAt;
    }
}