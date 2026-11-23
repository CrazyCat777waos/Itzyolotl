using Itzyolotl.Domain.Entities;
using Itzyolotl.Domain.Schedule;

namespace Itzyolotl.Domain.Tests.Schedule;

public class OpeningScheduleTests
{
    private static readonly TimeOnly Abre = new(9, 0);
    private static readonly TimeOnly Cierra = new(20, 0);

    private static List<OpeningPeriod> LunesASabado() =>
    [
        new(DayOfWeek.Monday, Abre, Cierra),
        new(DayOfWeek.Tuesday, Abre, Cierra),
        new(DayOfWeek.Wednesday, Abre, Cierra),
        new(DayOfWeek.Thursday, Abre, Cierra),
        new(DayOfWeek.Friday, Abre, Cierra),
        new(DayOfWeek.Saturday, Abre, Cierra)
    ];

    [Fact]
    public void Lunes_antes_de_abrir_esta_cerrado_y_abre_hoy()
    {
        var status = OpeningSchedule.Evaluate(LunesASabado(), new DateTime(2026, 11, 23, 8, 59, 0));

        Assert.False(status.IsOpen);
        Assert.Equal(new DateTime(2026, 11, 23, 9, 0, 0), status.NextChangeLocal);
    }

    [Fact]
    public void Lunes_justo_a_la_hora_de_abrir_esta_abierto_y_cierra_hoy()
    {
        var status = OpeningSchedule.Evaluate(LunesASabado(), new DateTime(2026, 11, 23, 9, 0, 0));

        Assert.True(status.IsOpen);
        Assert.Equal(new DateTime(2026, 11, 23, 20, 0, 0), status.NextChangeLocal);
    }

    [Fact]
    public void Lunes_un_minuto_antes_de_cerrar_sigue_abierto()
    {
        var status = OpeningSchedule.Evaluate(LunesASabado(), new DateTime(2026, 11, 23, 19, 59, 0));

        Assert.True(status.IsOpen);
    }

    [Fact]
    public void Lunes_justo_a_la_hora_de_cerrar_esta_cerrado_y_abre_manana()
    {
        var status = OpeningSchedule.Evaluate(LunesASabado(), new DateTime(2026, 11, 23, 20, 0, 0));

        Assert.False(status.IsOpen);
        Assert.Equal(new DateTime(2026, 11, 24, 9, 0, 0), status.NextChangeLocal);
    }

    [Fact]
    public void Sabado_por_la_noche_la_proxima_apertura_salta_el_domingo()
    {
        var status = OpeningSchedule.Evaluate(LunesASabado(), new DateTime(2026, 11, 28, 21, 0, 0));

        Assert.False(status.IsOpen);
        Assert.Equal(new DateTime(2026, 11, 30, 9, 0, 0), status.NextChangeLocal);
    }

    [Fact]
    public void Domingo_esta_cerrado_y_abre_el_lunes()
    {
        var status = OpeningSchedule.Evaluate(LunesASabado(), new DateTime(2026, 11, 29, 12, 0, 0));

        Assert.False(status.IsOpen);
        Assert.Equal(new DateTime(2026, 11, 30, 9, 0, 0), status.NextChangeLocal);
    }

    [Fact]
    public void Sin_tramos_siempre_esta_cerrado_y_no_hay_proximo_cambio()
    {
        var status = OpeningSchedule.Evaluate(new List<OpeningPeriod>(), new DateTime(2026, 11, 23, 12, 0, 0));

        Assert.False(status.IsOpen);
        Assert.Null(status.NextChangeLocal);
    }

    [Fact]
    public void Horario_partido_entre_tramos_esta_cerrado_y_abre_en_la_tarde()
    {
        List<OpeningPeriod> periods =
        [
            new(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(13, 0)),
            new(DayOfWeek.Monday, new TimeOnly(16, 0), new TimeOnly(20, 0))
        ];

        var status = OpeningSchedule.Evaluate(periods, new DateTime(2026, 11, 23, 14, 0, 0));

        Assert.False(status.IsOpen);
        Assert.Equal(new DateTime(2026, 11, 23, 16, 0, 0), status.NextChangeLocal);
    }

    [Fact]
    public void Si_solo_abre_un_dia_y_ya_cerro_la_proxima_apertura_es_la_semana_siguiente()
    {
        List<OpeningPeriod> periods = [new(DayOfWeek.Wednesday, Abre, Cierra)];

        var status = OpeningSchedule.Evaluate(periods, new DateTime(2026, 11, 25, 21, 0, 0));

        Assert.False(status.IsOpen);
        Assert.Equal(new DateTime(2026, 12, 2, 9, 0, 0), status.NextChangeLocal);
    }
}