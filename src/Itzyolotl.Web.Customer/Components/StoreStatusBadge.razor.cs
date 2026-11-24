using System.Text.Json;

using Itzyolotl.Contracts.Store;
using Itzyolotl.Web.Customer.Formatting;
using Itzyolotl.Web.Customer.Services;

using Microsoft.AspNetCore.Components;

namespace Itzyolotl.Web.Customer.Components;

public partial class StoreStatusBadge : IDisposable
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(60);

    private readonly CancellationTokenSource cts = new();

    [Inject]
    private StoreApiClient StoreApi { get; set; } = default!;

    private StoreStatusDto? status;
    private bool failed;

    private string? Detail => status?.NextChangeAt is { } next
        ? SpanishText.NextChange(status.IsOpen, next, DateTimeOffset.UtcNow)
        : null;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
        _ = RefreshLoopAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            status = await StoreApi.GetStatusAsync(cts.Token);
            failed = status is null;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            failed = status is null;
        }
    }

    private async Task RefreshLoopAsync()
    {
        using var timer = new PeriodicTimer(RefreshInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(cts.Token))
            {
                await LoadAsync();
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
    }
}