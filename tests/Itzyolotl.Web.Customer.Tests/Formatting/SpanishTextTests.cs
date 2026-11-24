using Itzyolotl.Web.Customer.Formatting;

namespace Itzyolotl.Web.Customer.Tests.Formatting;

public class SpanishTextTests
{
    [Theory]
    [InlineData("lunes", "Lunes")]
    [InlineData("miércoles", "Miércoles")]
    [InlineData("sábado", "Sábado")]
    [InlineData("Domingo", "Domingo")]
    [InlineData("a", "A")]
    [InlineData("jueves santo", "Jueves santo")]
    public void Capitalized_pone_en_mayuscula_solo_la_primera_letra(string texto, string esperado)
    {
        Assert.Equal(esperado, SpanishText.Capitalized(texto));
    }

    [Fact]
    public void Capitalized_con_cadena_vacia_devuelve_cadena_vacia()
    {
        Assert.Equal(string.Empty, SpanishText.Capitalized(string.Empty));
    }
    [Theory]
    [InlineData(DayOfWeek.Sunday, "domingo")]
    [InlineData(DayOfWeek.Monday, "lunes")]
    [InlineData(DayOfWeek.Tuesday, "martes")]
    [InlineData(DayOfWeek.Wednesday, "miércoles")]
    [InlineData(DayOfWeek.Thursday, "jueves")]
    [InlineData(DayOfWeek.Friday, "viernes")]
    [InlineData(DayOfWeek.Saturday, "sábado")]
    public void DayName_devuelve_el_nombre_en_espanol(DayOfWeek dia, string esperado)
    {
        Assert.Equal(esperado, SpanishText.DayName(dia));
    }

    [Theory]
    [InlineData(0, 0, "12:00 a. m.")]
    [InlineData(0, 5, "12:05 a. m.")]
    [InlineData(9, 30, "9:30 a. m.")]
    [InlineData(11, 59, "11:59 a. m.")]
    [InlineData(12, 0, "12:00 p. m.")]
    [InlineData(12, 30, "12:30 p. m.")]
    [InlineData(13, 0, "1:00 p. m.")]
    [InlineData(19, 0, "7:00 p. m.")]
    [InlineData(23, 59, "11:59 p. m.")]
    public void Time_usa_formato_de_12_horas(int hora, int minuto, string esperado)
    {
        Assert.Equal(esperado, SpanishText.Time(new TimeOnly(hora, minuto)));
    }

    [Fact]
    public void Time_con_DateTimeOffset_usa_la_hora_local_y_no_la_UTC()
    {
        var hora = new DateTimeOffset(2026, 11, 24, 19, 0, 0, TimeSpan.FromHours(-6));

        Assert.Equal("7:00 p. m.", SpanishText.Time(hora));
    }

    [Fact]
    public void Range_une_apertura_y_cierre_con_guion_largo()
    {
        var rango = SpanishText.Range(new TimeOnly(19, 0), new TimeOnly(23, 0));

        Assert.Equal("7:00 p. m. \u2013 11:00 p. m.", rango);
    }
    private static DateTimeOffset Utc(int dia, int hora, int minuto) =>
    new(2026, 11, dia, hora, minuto, 0, TimeSpan.Zero);

    private static DateTimeOffset Mexico(int dia, int hora, int minuto) =>
        new(2026, 11, dia, hora, minuto, 0, TimeSpan.FromHours(-6));

    [Fact]
    public void NextChange_abierto_indica_la_hora_de_cierre()
    {
        var texto = SpanishText.NextChange(true, Mexico(24, 23, 0), Utc(25, 2, 0));

        Assert.Equal("Cierra a las 11:00 p. m.", texto);
    }

    [Fact]
    public void NextChange_cerrado_que_abre_el_mismo_dia_dice_hoy()
    {
        // Martes 24, 3:00 p. m. en Mexico (21:00 UTC); abre a las 7:00 p. m.
        var texto = SpanishText.NextChange(false, Mexico(24, 19, 0), Utc(24, 21, 0));

        Assert.Equal("Abre hoy a las 7:00 p. m.", texto);
    }

    [Fact]
    public void NextChange_cerrado_que_abre_al_dia_siguiente_dice_manana()
    {
        // Domingo 29, 12:00 p. m. en Mexico (18:00 UTC); abre el lunes 30.
        var texto = SpanishText.NextChange(false, Mexico(30, 19, 0), Utc(29, 18, 0));

        Assert.Equal("Abre mañana a las 7:00 p. m.", texto);
    }

    [Fact]
    public void NextChange_compara_los_dias_en_hora_local_y_no_en_UTC()
    {
        // Martes 24, 11:30 p. m. en Mexico = miercoles 25, 05:30 UTC.
        // En UTC "hoy" y la apertura caerian el mismo dia; en Mexico es manana.
        var texto = SpanishText.NextChange(false, Mexico(25, 19, 0), Utc(25, 5, 30));

        Assert.Equal("Abre mañana a las 7:00 p. m.", texto);
    }

    [Fact]
    public void NextChange_cerrado_con_apertura_en_dos_dias_o_mas_dice_el_dia_de_la_semana()
    {
        // Sabado 28, 11:30 p. m. en Mexico (domingo 29, 05:30 UTC); abre el lunes 30.
        var texto = SpanishText.NextChange(false, Mexico(30, 19, 0), Utc(29, 5, 30));

        Assert.Equal("Abre el lunes a las 7:00 p. m.", texto);
    }
}