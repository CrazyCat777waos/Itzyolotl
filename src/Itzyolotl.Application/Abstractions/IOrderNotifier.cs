namespace Itzyolotl.Application.Abstractions;

public interface IOrderNotifier
{
    Task NotifyQueueChangedAsync(CancellationToken cancellationToken = default);
}