namespace Itzyolotl.Web.Customer.Formatting;

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

    private static string Time(int hour, int minute)
    {
        var hour12 = hour % 12 == 0 ? 12 : hour % 12;
        var suffix = hour < 12 ? "a. m." : "p. m.";

        return $"{hour12}:{minute:00} {suffix}";
    }
}