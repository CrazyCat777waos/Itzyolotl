using Itzyolotl.Contracts.Orders;
using Itzyolotl.Web.Customer.Services;

using Microsoft.AspNetCore.Components;

namespace Itzyolotl.Web.Customer.Pages;

public partial class OrderTracking : IDisposable
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);

    private static readonly (string Label, OrderStatusDto ReachedAt)[] stages =
    [
        ("Pago confirmado", OrderStatusDto.Paid),
        ("En preparación", OrderStatusDto.InPreparation),
        ("Listo para entregar", OrderStatusDto.Ready),
        ("Entregado", OrderStatusDto.Delivered)
    ];

    private readonly CancellationTokenSource cts = new();

    [Parameter]
    public Guid Id { get; set; }

    [Inject]
    private OrdersApiClient OrdersApi { get; set; } = default!;

    private OrderDto? order;
    private bool notFound;
    private string? error;

    private bool IsFinished =>
        order?.Status is OrderStatusDto.Delivered or OrderStatusDto.Cancelled;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();

        _ = PollAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var result = await OrdersApi.GetAsync(Id, cts.Token);

            if (result is null)
            {
                notFound = true;
            }
            else
            {
                order = result;
                error = null;
            }
        }
        catch (HttpRequestException)
        {
            error = order is null
                ? "No se pudo cargar tu pedido. Reintentando..."
                : "Sin conexión. Mostramos la última información disponible.";
        }
    }

    private async Task PollAsync()
    {
        using var timer = new PeriodicTimer(PollInterval);

        try
        {
            while (!notFound && !IsFinished && await timer.WaitForNextTickAsync(cts.Token))
            {
                await LoadAsync();

                if (cts.IsCancellationRequested)
                {
                    return;
                }

                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
            // La pagina se cerro: es el final normal del ciclo.
        }
    }

    public void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
    }
}