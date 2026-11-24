using Itzyolotl.Contracts.Store;
using Itzyolotl.Web.Customer.Formatting;
using Itzyolotl.Web.Customer.Services;

using Microsoft.AspNetCore.Components;

namespace Itzyolotl.Web.Customer.Components;

public partial class WeeklySchedule : IDisposable
{
    [Inject]
    private StoreStatusState State { get; set; } = default!;

    private StoreStatusDto? Status => State.Status;

    private List<DayScheduleDto> OrderedDays =>
        Status?.WeeklySchedule.OrderBy(d => ((int)d.Day + 6) % 7).ToList() ?? [];

    protected override async Task OnInitializedAsync()
    {
        State.Changed += OnStateChanged;
        await State.EnsureStartedAsync();
    }

    private void OnStateChanged() => _ = InvokeAsync(StateHasChanged);

    private static string DayLabel(DayOfWeek day) =>
        SpanishText.Capitalized(SpanishText.DayName(day));

    private static string HoursText(DayScheduleDto day) =>
        day.Periods.Count == 0
            ? "Cerrado"
            : string.Join(", ", day.Periods.Select(p => SpanishText.Range(p.OpensAt, p.ClosesAt)));

    public void Dispose() => State.Changed -= OnStateChanged;
}