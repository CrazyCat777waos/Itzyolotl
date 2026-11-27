using Itzyolotl.Application.Abstractions;

using Microsoft.AspNetCore.SignalR;

namespace Itzyolotl.Api.Hubs;

public sealed class SignalROrderNotifier(
    IHubContext<OrdersHub> hub,
    ILogger<SignalROrderNotifier> logger) : IOrderNotifier
{
    public async Task NotifyQueueChangedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await hub.Clients
                .Group(OrdersHub.StaffGroup)
                .SendAsync(OrdersHub.QueueChangedEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            // Un aviso fallido no debe deshacer un pedido que ya se guardo.
            logger.LogWarning(ex, "No se pudo avisar a la cola del personal.");
        }
    }
}