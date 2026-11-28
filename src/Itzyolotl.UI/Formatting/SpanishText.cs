namespace Itzyolotl.UI.Formatting;
using Itzyolotl.Contracts.Orders;

public static class SpanishText
{
    private static readonly string[] DayNames =
        ["domingo", "lunes", "martes", "miércoles", "jueves", "viernes", "sábado"];

    public static string DayName(DayOfWeek day) => DayNames[(int)day];

    public static string Capitalized(string text) =>
    text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text[1..];

    public static string Range(TimeOnly opensAt, TimeOnly closesAt) =>
        $"{Time(opensAt)} – {Time(closesAt)}";

    public static string Time(TimeOnly time) => Time(time.Hour, time.Minute);

    public static string Time(DateTimeOffset time) => Time(time.Hour, time.Minute);

    public static string NextChange(bool isOpen, DateTimeOffset next, DateTimeOffset nowUtc)
    {
        if (isOpen)
        {
            return $"Cierra a las {Time(next)}";
        }

        var localNow = nowUtc.ToOffset(next.Offset);
        var daysAway = (next.Date - localNow.Date).Days;

        var when = daysAway switch
        {
            <= 0 => "hoy",
            1 => "mañana",
            _ => $"el {DayName(next.DayOfWeek)}"
        };

        return $"Abre {when} a las {Time(next)}";
    }
    public static string OrderStatusLabel(OrderStatusDto status) => status switch
    {
        OrderStatusDto.PendingPayment => "Esperando confirmación de pago",
        OrderStatusDto.Paid => "Pago confirmado",
        OrderStatusDto.InPreparation => "Preparando tu pedido",
        OrderStatusDto.Ready => "¡Tu pedido está listo!",
        OrderStatusDto.Delivered => "Entregado",
        OrderStatusDto.Cancelled => "Pedido cancelado",
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };

    public static string ServiceTypeLabel(ServiceTypeDto serviceType) => serviceType switch
    {
        ServiceTypeDto.DineIn => "Para comer aquí",
        ServiceTypeDto.TakeAway => "Para llevar",
        _ => throw new ArgumentOutOfRangeException(nameof(serviceType))
    };

    private static string Time(int hour, int minute)
    {
        var hour12 = hour % 12 == 0 ? 12 : hour % 12;
        var suffix = hour < 12 ? "a. m." : "p. m.";

        return $"{hour12}:{minute:00} {suffix}";
    }
    public static string Elapsed(DateTimeOffset since, DateTimeOffset now)
    {
        var minutes = (int)Math.Max(0, (now - since).TotalMinutes);

        return minutes switch
        {
            < 1 => "ahora mismo",
            < 60 => $"hace {minutes} min",
            _ when minutes % 60 == 0 => $"hace {minutes / 60} h",
            _ => $"hace {minutes / 60} h {minutes % 60} min"
        };
    }

}