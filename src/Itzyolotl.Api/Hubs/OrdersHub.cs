using Microsoft.AspNetCore.SignalR;

namespace Itzyolotl.Api.Hubs;

public sealed class OrdersHub : Hub
{
    public const string StaffGroup = "staff";
    public const string QueueChangedEvent = "QueueChanged";

    public Task JoinStaff() =>
        Groups.AddToGroupAsync(Context.ConnectionId, StaffGroup);
}