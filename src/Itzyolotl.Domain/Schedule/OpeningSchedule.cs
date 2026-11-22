using Itzyolotl.Domain.Entities;

namespace Itzyolotl.Domain.Schedule;

public sealed record StoreStatus(bool IsOpen, DateTime? NextChangeLocal);

public static class OpeningSchedule
{
    public static StoreStatus Evaluate(
        IReadOnlyCollection<OpeningPeriod> periods,
        DateTime localNow)
    {
        var today = localNow.Date;
        var time = TimeOnly.FromDateTime(localNow);

        var current = periods.FirstOrDefault(p =>
            p.Day == localNow.DayOfWeek && p.OpensAt <= time && time < p.ClosesAt);

        if (current is not null)
        {
            return new StoreStatus(true, today.Add(current.ClosesAt.ToTimeSpan()));
        }

        for (var offset = 0; offset <= 7; offset++)
        {
            var date = today.AddDays(offset);

            var next = periods
                .Where(p => p.Day == date.DayOfWeek && (offset > 0 || p.OpensAt > time))
                .OrderBy(p => p.OpensAt)
                .FirstOrDefault();

            if (next is not null)
            {
                return new StoreStatus(false, date.Add(next.OpensAt.ToTimeSpan()));
            }
        }

        return new StoreStatus(false, null);
    }
}