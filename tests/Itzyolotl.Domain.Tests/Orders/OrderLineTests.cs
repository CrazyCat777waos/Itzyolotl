using Itzyolotl.Domain.Entities;

namespace Itzyolotl.Domain.Tests.Orders;

public class OrderLineTests
{
    [Fact]
    public void Subtotal_multiplica_precio_por_cantidad()
    {
        var linea = new OrderLine(1, "Capuchino", 45.50m, 2);

        Assert.Equal(91m, linea.Subtotal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Cantidad_cero_o_negativa_es_invalida(int cantidad)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new OrderLine(1, "Capuchino", 45m, cantidad));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Precio_cero_o_negativo_es_invalido(int precio)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new OrderLine(1, "Capuchino", precio, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Producto_con_id_invalido_es_invalido(int productoId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new OrderLine(productoId, "Capuchino", 45m, 1));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Nombre_de_producto_vacio_es_invalido(string nombre)
    {
        Assert.Throws<ArgumentException>(
            () => new OrderLine(1, nombre, 45m, 1));
    }

    [Fact]
    public void La_nota_se_recorta()
    {
        var linea = new OrderLine(1, "Crepa", 60m, 1, "  muy dorada  ");

        Assert.Equal("muy dorada", linea.Note);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Nota_vacia_o_nula_queda_como_null(string? nota)
    {
        var linea = new OrderLine(1, "Crepa", 60m, 1, nota);

        Assert.Null(linea.Note);
    }

    [Fact]
    public void Nota_de_exactamente_el_maximo_es_valida()
    {
        var nota = new string('a', OrderLine.MaxNoteLength);

        var linea = new OrderLine(1, "Crepa", 60m, 1, nota);

        Assert.Equal(nota, linea.Note);
    }

    [Fact]
    public void Nota_que_supera_el_maximo_es_invalida()
    {
        var nota = new string('a', OrderLine.MaxNoteLength + 1);

        Assert.Throws<ArgumentException>(
            () => new OrderLine(1, "Crepa", 60m, 1, nota));
    }
}