using Itzyolotl.Contracts.Orders;
using Itzyolotl.Web.Staff.Services;

using Microsoft.AspNetCore.Components;

namespace Itzyolotl.Web.Staff.Pages;

public partial class OrderQueue : IDisposable
{
    private sealed record Column(
        string Title,
        OrderStatusDto Status,
        string ActionLabel,
        OrderAction Action,
        bool CanCancel);

    private static readonly Column[] columns =
    [
        new("Por cobrar", OrderStatusDto.PendingPayment, "Confirmar pago", OrderAction.MarkPaid, true),
        new("En cola", OrderStatusDto.Paid, "Preparar", OrderAction.StartPreparation, true),
        new("Preparando", OrderStatusDto.InPreparation, "Marcar listo", OrderAction.MarkReady, false),
        new("Listos", OrderStatusDto.Ready, "Entregar", OrderAction.MarkDelivered, false)
    ];

    [Inject]
    private StaffQueueState State { get; set; } = default!;

    [Inject]
    private StaffApiClient Api { get; set; } = default!;

    [Inject]
    private StaffAccess Access { get; set; } = default!;

    private Guid? busyId;
    private Guid? cancelingId;
    private string cancelReason = string.Empty;
    private string? message;

    protected override async Task OnInitializedAsync()
    {
        State.Changed += OnStateChanged;

        await State.StartAsync();
    }

    private void StartCanceling(OrderDto order)
    {
        cancelingId = order.Id;
        cancelReason = string.Empty;
        message = null;
    }

    private void StopCanceling()
    {
        cancelingId = null;
        cancelReason = string.Empty;
    }

    private async Task ApplyAsync(OrderDto order, OrderAction action, string? reason = null)
    {
        busyId = order.Id;
        message = null;

        try
        {
            var result = await Api.ApplyActionAsync(order.Id, action, reason);

            if (result.Succeeded)
            {
                StopCanceling();
            }
            else
            {
                message = $"Pedido #{order.Number}: {result.Error}";
            }

            await State.RefreshAsync();
        }
        catch (UnauthorizedAccessException)
        {
            await Access.SignOutAsync("La clave no es correcta.");
        }
        catch (HttpRequestException)
        {
            message = "No se pudo conectar con el servidor. Inténtalo de nuevo.";
        }
        finally
        {
            busyId = null;
        }
    }

    private void OnStateChanged() => _ = InvokeAsync(StateHasChanged);

    public void Dispose() => State.Changed -= OnStateChanged;
}