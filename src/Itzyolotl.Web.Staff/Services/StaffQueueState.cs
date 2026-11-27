using Itzyolotl.Contracts.Orders;

using Microsoft.AspNetCore.SignalR.Client;

namespace Itzyolotl.Web.Staff.Services;

public sealed class StaffQueueState : IAsyncDisposable
{
    private static readonly TimeSpan FallbackPollInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    private readonly StaffApiClient api;
    private readonly StaffAccess access;
    private readonly HubConnection connection;

    private CancellationTokenSource? cts;
    private int refreshTicket;
    private int appliedTicket;

    public StaffQueueState(HttpClient http, StaffApiClient api, StaffAccess access)
    {
        this.api = api;
        this.access = access;

        connection = new HubConnectionBuilder()
            .WithUrl(new Uri(http.BaseAddress!, OrdersHubNames.Path))
            .WithAutomaticReconnect(new AlwaysRetryPolicy(RetryDelay))
            .Build();

        connection.On(OrdersHubNames.QueueChanged, async () => await RefreshAsync());

        connection.Reconnecting += _ =>
        {
            SetLive(false);
            return Task.CompletedTask;
        };

        connection.Reconnected += async _ =>
        {
            await JoinAsync(CancellationToken.None);
            SetLive(true);
            await RefreshAsync();
        };

        connection.Closed += _ =>
        {
            SetLive(false);
            return Task.CompletedTask;
        };

        access.Changed += OnAccessChanged;
    }

    public event Action? Changed;

    public IReadOnlyList<OrderDto> Orders { get; private set; } = [];

    public bool HasLoaded { get; private set; }

    public bool IsLive { get; private set; }

    public string? LastError { get; private set; }

    public Task StartAsync()
    {
        if (cts is not null || !access.HasKey)
        {
            return Task.CompletedTask;
        }

        cts = new CancellationTokenSource();
        _ = RunAsync(cts.Token);

        return Task.CompletedTask;
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var ticket = ++refreshTicket;

        try
        {
            var result = await api.GetQueueAsync(cancellationToken);

            // Si una respuesta mas nueva ya se aplico, esta llego tarde y se descarta.
            if (ticket < appliedTicket)
            {
                return;
            }

            appliedTicket = ticket;
            Orders = result;
            HasLoaded = true;
            LastError = null;
        }
        catch (UnauthorizedAccessException)
        {
            await access.SignOutAsync("La clave no es correcta.");
            return;
        }
        catch (HttpRequestException)
        {
            LastError = "Sin conexión con el servidor.";
        }

        Changed?.Invoke();
    }

    public async Task StopAsync()
    {
        if (cts is null)
        {
            return;
        }

        await cts.CancelAsync();
        cts.Dispose();
        cts = null;

        await connection.StopAsync();

        Orders = [];
        HasLoaded = false;
        IsLive = false;
        LastError = null;

        Changed?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        access.Changed -= OnAccessChanged;

        await StopAsync();
        await connection.DisposeAsync();
    }

    private async Task RunAsync(CancellationToken token)
    {
        try
        {
            await RefreshAsync(token);
            await ConnectAsync(token);

            using var timer = new PeriodicTimer(FallbackPollInterval);

            while (await timer.WaitForNextTickAsync(token))
            {
                await RefreshAsync(token);
            }
        }
        catch (OperationCanceledException)
        {
            // Se detuvo la cola: es el final normal del ciclo.
        }
    }

    private async Task ConnectAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (connection.State == HubConnectionState.Disconnected)
                {
                    await connection.StartAsync(token);
                }

                await JoinAsync(token);
                SetLive(true);

                // Cierra el hueco entre la primera consulta y la union al canal.
                await RefreshAsync(token);

                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                SetLive(false);
                await Task.Delay(RetryDelay, token);
            }
        }
    }

    private Task JoinAsync(CancellationToken token) =>
        connection.InvokeAsync(OrdersHubNames.JoinStaff, token);

    private void SetLive(bool value)
    {
        IsLive = value;
        Changed?.Invoke();
    }

    private void OnAccessChanged()
    {
        if (!access.HasKey)
        {
            _ = StopAsync();
        }
    }

    private sealed class AlwaysRetryPolicy(TimeSpan delay) : IRetryPolicy
    {
        public TimeSpan? NextRetryDelay(RetryContext retryContext) => delay;
    }
}