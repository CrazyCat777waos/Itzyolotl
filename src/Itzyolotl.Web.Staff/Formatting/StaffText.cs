using Itzyolotl.Contracts.Orders;

namespace Itzyolotl.Web.Staff.Formatting;

public static class StaffText
{
    public static string ServiceTypeLabel(ServiceTypeDto serviceType) => serviceType switch
    {
        ServiceTypeDto.DineIn => "Para comer aquí",
        ServiceTypeDto.TakeAway => "Para llevar",
        _ => throw new ArgumentOutOfRangeException(nameof(serviceType))
    };

    public static string Elapsed(DateTimeOffset since, DateTimeOffset now)
    {
        var minutes = (int)Math.Max(0, (now - since).TotalMinutes);

        return minutes switch
        {
            < 1 => "ahora mismo",
            < 60 => $"hace {minutes} min",
            _ => $"hace {minutes / 60} h {minutes % 60} min"
        };
    }
}