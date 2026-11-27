using Itzyolotl.Web.Staff.Services;

using Microsoft.AspNetCore.Components;

namespace Itzyolotl.Web.Staff.Layout;

public partial class StaffLayout : IDisposable
{
    [Inject]
    private StaffAccess Access { get; set; } = default!;

    [Inject]
    private StaffQueueState State { get; set; } = default!;

    private bool loaded;

    protected override async Task OnInitializedAsync()
    {
        Access.Changed += OnChanged;
        State.Changed += OnChanged;

        await Access.EnsureLoadedAsync();

        loaded = true;
    }

    private Task SignOutAsync() => Access.SignOutAsync();

    private void OnChanged() => _ = InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        Access.Changed -= OnChanged;
        State.Changed -= OnChanged;
    }
}