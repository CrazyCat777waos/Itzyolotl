using Itzyolotl.Contracts.Store;
using Itzyolotl.Web.Customer.Formatting;
using Itzyolotl.Web.Customer.Services;

using Microsoft.AspNetCore.Components;

namespace Itzyolotl.Web.Customer.Components;

public partial class StoreStatusBadge : IDisposable
{
    [Inject]
    private StoreStatusState State { get; set; } = default!;

    private StoreStatusDto? Status => State.Status;

    private bool Failed => State.Failed;

    private string? Detail
    {
        get
        {
            var current = Status;

            return current?.NextChangeAt is { } next
                ? SpanishText.NextChange(current.IsOpen, next, DateTimeOffset.UtcNow)
                : null;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        State.Changed += OnStateChanged;
        await State.EnsureStartedAsync();
    }

    private void OnStateChanged() => _ = InvokeAsync(StateHasChanged);

    public void Dispose() => State.Changed -= OnStateChanged;
}