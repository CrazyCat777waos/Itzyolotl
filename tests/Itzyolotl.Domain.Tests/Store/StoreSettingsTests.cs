using Itzyolotl.Domain.Entities;

namespace Itzyolotl.Domain.Tests.Store;

public class StoreSettingsTests
{
    [Fact]
    public void La_configuracion_siempre_usa_el_id_unico()
    {
        var settings = new StoreSettings(2);

        Assert.Equal(StoreSettings.SingletonId, settings.Id);
        Assert.Equal(2, settings.Capacity);
    }

    [Theory]
    [InlineData(StoreSettings.MinCapacity)]
    [InlineData(StoreSettings.MaxCapacity)]
    public void Capacidad_en_los_limites_es_valida(int capacidad)
    {
        var settings = new StoreSettings(capacidad);

        Assert.Equal(capacidad, settings.Capacity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(StoreSettings.MaxCapacity + 1)]
    public void Capacidad_fuera_de_rango_es_invalida(int capacidad)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new StoreSettings(capacidad));
    }

    [Fact]
    public void Se_puede_cambiar_la_capacidad()
    {
        var settings = new StoreSettings(2);

        settings.ChangeCapacity(4);

        Assert.Equal(4, settings.Capacity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(StoreSettings.MaxCapacity + 1)]
    public void Cambiar_a_una_capacidad_invalida_lanza_excepcion(int capacidad)
    {
        var settings = new StoreSettings(2);

        Assert.Throws<ArgumentOutOfRangeException>(() => settings.ChangeCapacity(capacidad));
    }
}