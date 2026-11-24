using System.Text.Json;

using Itzyolotl.Contracts.Store;

namespace Itzyolotl.Web.Customer.Services;

public sealed class StoreStatusState(StoreApiClient api) : IDisposable
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(60);

    private readonly CancellationTokenSource cts = new();
    private Task? started;

    public StoreStatusDto? Status { get; private set; }

    public bool Failed { get; private set; }

    public event Action? Changed;

    public Task EnsureStartedAsync() => started ??= StartAsync();

    private async Task StartAsync()
    {
        await RefreshAsync();
        _ = RefreshLoopAsync();
    }

    private async Task RefreshAsync()
    {
        try
        {
            var result = await api.GetStatusAsync(cts.Token);

            if (result is not null)
            {
                Status = result;
            }

            Failed = Status is null;
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            Failed = Status is null;
        }

        Changed?.Invoke();
    }

    private async Task RefreshLoopAsync()
    {
        var token = cts.Token;
        using var timer = new PeriodicTimer(RefreshInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(token))
            {
                await RefreshAsync();
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