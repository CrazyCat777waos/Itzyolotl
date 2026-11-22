using Itzyolotl.Application.Abstractions;
using Itzyolotl.Contracts.Store;
using Itzyolotl.Domain.Schedule;

using Microsoft.EntityFrameworkCore;

namespace Itzyolotl.Application.Store;

public sealed class GetStoreStatusHandler(IAppDbContext db, TimeProvider timeProvider)
{
    private static readonly TimeZoneInfo StoreTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");

    public async Task<StoreStatusDto> HandleAsync(CancellationToken cancellationToken)
    {
        var periods = await db.OpeningPeriods
            .AsNoTracking()
            .OrderBy(p => p.Day)
            .ThenBy(p => p.OpensAt)
            .ToListAsync(cancellationToken);

        var localNow = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), StoreTimeZone).DateTime;
        var status = OpeningSchedule.Evaluate(periods, localNow);

        DateTimeOffset? nextChangeAt = status.NextChangeLocal is { } next
            ? new DateTimeOffset(next, StoreTimeZone.GetUtcOffset(next))
            : null;

        var weeklySchedule = Enum.GetValues<DayOfWeek>()
            .Select(day => new DayScheduleDto(
                day,
                periods
                    .Where(p => p.Day == day)
                    .Select(p => new OpeningPeriodDto(p.OpensAt, p.ClosesAt))
                    .ToList()))
            .ToList();

        return new StoreStatusDto(status.IsOpen, nextChangeAt, StoreTimeZone.Id, weeklySchedule);
    }
}