using Itzyolotl.Domain.Entities;

namespace Itzyolotl.Domain.Tests.Orders;

public class OrderLineTests
{
    private const PreparationStation Estacion = PreparationStation.CoffeeAndPaninis;

    [Fact]
    public void Subtotal_multiplica_precio_por_cantidad()
    {
        var linea = new OrderLine(1, "Capuchino", 45.50m, 2, Estacion, 5);

        Assert.Equal(91m, linea.Subtotal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Cantidad_cero_o_negativa_es_invalida(int cantidad)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new OrderLine(1, "Capuchino", 45m, cantidad, Estacion, 5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Precio_cero_o_negativo_es_invalido(int precio)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new OrderLine(1, "Capuchino", precio, 1, Estacion, 5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Producto_con_id_invalido_es_invalido(int productoId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new OrderLine(productoId, "Capuchino", 45m, 1, Estacion, 5));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Nombre_de_producto_vacio_es_invalido(string nombre)
    {
        Assert.Throws<ArgumentException>(
            () => new OrderLine(1, nombre, 45m, 1, Estacion, 5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Con_estacion_de_preparacion_los_minutos_deben_ser_positivos(int minutos)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new OrderLine(1, "Capuchino", 45m, 1, Estacion, minutos));
    }

    [Fact]
    public void Estacion_y_minutos_se_guardan_en_la_linea()
    {
        var linea = new OrderLine(1, "Crepa", 60m, 1, PreparationStation.Crepes, 12);

        Assert.Equal(PreparationStation.Crepes, linea.Station);
        Assert.Equal(12, linea.PreparationMinutes);
    }

    [Fact]
    public void Estacion_inexistente_es_invalida()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new OrderLine(1, "Crepa", 60m, 1, (PreparationStation)99, 5));
    }

    [Fact]
    public void Producto_sin_preparacion_con_cero_minutos_es_valido()
    {
        var linea = new OrderLine(1, "Coca-Cola (lata)", 30m, 1, PreparationStation.None, 0);

        Assert.Equal(PreparationStation.None, linea.Station);
        Assert.Equal(0, linea.PreparationMinutes);
    }

    [Fact]
    public void Producto_sin_preparacion_con_minutos_es_invalido()
    {
        Assert.Throws<ArgumentException>(
            () => new OrderLine(1, "Coca-Cola (lata)", 30m, 1, PreparationStation.None, 3));
    }

    [Fact]
    public void La_nota_se_recorta()
    {
        var linea = new OrderLine(1, "Crepa", 60m, 1, PreparationStation.Crepes, 5, "  muy dorada  ");

        Assert.Equal("muy dorada", linea.Note);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Nota_vacia_o_nula_queda_como_null(string? nota)
    {
        var linea = new OrderLine(1, "Crepa", 60m, 1, PreparationStation.Crepes, 5, nota);

        Assert.Null(linea.Note);
    }

    [Fact]
    public void Nota_de_exactamente_el_maximo_es_valida()
    {
        var nota = new string('a', OrderLine.MaxNoteLength);

        var linea = new OrderLine(1, "Crepa", 60m, 1, PreparationStation.Crepes, 5, nota);

        Assert.Equal(nota, linea.Note);
    }

    [Fact]
    public void Nota_que_supera_el_maximo_es_invalida()
    {
        var nota = new string('a', OrderLine.MaxNoteLength + 1);

        Assert.Throws<ArgumentException>(
            () => new OrderLine(1, "Crepa", 60m, 1, PreparationStation.Crepes, 5, nota));
    }
}