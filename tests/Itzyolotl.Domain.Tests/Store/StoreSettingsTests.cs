using Itzyolotl.Domain.Entities;

namespace Itzyolotl.Domain.Tests.Store;

public class StoreSettingsTests
{
    [Fact]
    public void La_configuracion_usa_el_id_unico_y_guarda_cada_capacidad()
    {
        var settings = new StoreSettings(1, 2, 3);

        Assert.Equal(StoreSettings.SingletonId, settings.Id);
        Assert.Equal(1, settings.CapacityFor(PreparationStation.Crepes));
        Assert.Equal(2, settings.CapacityFor(PreparationStation.CoffeeAndPaninis));
        Assert.Equal(3, settings.CapacityFor(PreparationStation.Back));
    }

    [Theory]
    [InlineData(StoreSettings.MinCapacity)]
    [InlineData(StoreSettings.MaxCapacity)]
    public void Capacidad_en_los_limites_es_valida(int capacidad)
    {
        var settings = new StoreSettings(capacidad, capacidad, capacidad);

        Assert.Equal(capacidad, settings.CapacityFor(PreparationStation.Crepes));
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1, 0, 1)]
    [InlineData(1, 1, 0)]
    [InlineData(StoreSettings.MaxCapacity + 1, 1, 1)]
    [InlineData(1, StoreSettings.MaxCapacity + 1, 1)]
    [InlineData(1, 1, StoreSettings.MaxCapacity + 1)]
    public void Cualquier_capacidad_fuera_de_rango_es_invalida(int crepas, int cafe, int atras)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new StoreSettings(crepas, cafe, atras));
    }

    [Fact]
    public void Consultar_la_capacidad_de_sin_preparacion_lanza_excepcion()
    {
        var settings = new StoreSettings(1, 1, 1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => settings.CapacityFor(PreparationStation.None));
    }

    [Theory]
    [InlineData(PreparationStation.Crepes)]
    [InlineData(PreparationStation.CoffeeAndPaninis)]
    [InlineData(PreparationStation.Back)]
    public void Cambiar_la_capacidad_de_una_estacion_no_afecta_a_las_demas(PreparationStation estacion)
    {
        var settings = new StoreSettings(1, 1, 1);

        settings.ChangeCapacity(estacion, 3);

        PreparationStation[] todas =
        [
            PreparationStation.Crepes,
            PreparationStation.CoffeeAndPaninis,
            PreparationStation.Back
        ];

        foreach (var otra in todas)
        {
            Assert.Equal(otra == estacion ? 3 : 1, settings.CapacityFor(otra));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(StoreSettings.MaxCapacity + 1)]
    public void Cambiar_a_una_capacidad_invalida_lanza_excepcion(int capacidad)
    {
        var settings = new StoreSettings(1, 1, 1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => settings.ChangeCapacity(PreparationStation.Crepes, capacidad));
    }

    [Fact]
    public void Cambiar_la_capacidad_de_sin_preparacion_lanza_excepcion()
    {
        var settings = new StoreSettings(1, 1, 1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => settings.ChangeCapacity(PreparationStation.None, 2));
    }
}