namespace Itzyolotl.Contracts.Store;

public sealed record OpeningPeriodDto(TimeOnly OpensAt, TimeOnly ClosesAt);

public sealed record DayScheduleDto(DayOfWeek Day, IReadOnlyList<OpeningPeriodDto> Periods);

public sealed record StoreStatusDto(
    bool IsOpen,
    DateTimeOffset? NextChangeAt,
    string TimeZoneId,
    IReadOnlyList<DayScheduleDto> WeeklySchedule);