using Itzyolotl.Domain.Entities;

namespace Itzyolotl.Domain.Tests.Orders;

public class OrderTests
{
    private static readonly DateTimeOffset Ahora =
        new(2026, 11, 24, 20, 0, 0, TimeSpan.FromHours(-6));

    private static OrderLine Crepa(int cantidad = 1) =>
        new(1, "Crepa de Nutella", 60m, cantidad);

    private static Order PedidoNuevo() =>
        new(Guid.NewGuid(), "Ana", ServiceType.DineIn, [Crepa()], Ahora);

    [Fact]
    public void Pedido_nuevo_queda_pendiente_de_pago_con_su_fecha()
    {
        var clave = Guid.NewGuid();

        var pedido = new Order(clave, "Ana", ServiceType.TakeAway, [Crepa()], Ahora);

        Assert.Equal(OrderStatus.PendingPayment, pedido.Status);
        Assert.Equal(clave, pedido.IdempotencyKey);
        Assert.Equal(ServiceType.TakeAway, pedido.ServiceType);
        Assert.Equal(Ahora, pedido.CreatedAt);
        Assert.Null(pedido.PaidAt);
    }

    [Fact]
    public void El_total_suma_los_subtotales_de_las_lineas()
    {
        var capuchino = new OrderLine(2, "Capuchino", 45m, 1);

        var pedido = new Order(
            Guid.NewGuid(), "Ana", ServiceType.DineIn, [Crepa(2), capuchino], Ahora);

        Assert.Equal(165m, pedido.Total);
    }

    [Fact]
    public void Pedido_sin_lineas_es_invalido()
    {
        Assert.Throws<ArgumentException>(
            () => new Order(Guid.NewGuid(), "Ana", ServiceType.DineIn, [], Ahora));
    }

    [Fact]
    public void Clave_de_idempotencia_vacia_es_invalida()
    {
        Assert.Throws<ArgumentException>(
            () => new Order(Guid.Empty, "Ana", ServiceType.DineIn, [Crepa()], Ahora));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Nombre_de_cliente_vacio_es_invalido(string nombre)
    {
        Assert.Throws<ArgumentException>(
            () => new Order(Guid.NewGuid(), nombre, ServiceType.DineIn, [Crepa()], Ahora));
    }

    [Fact]
    public void El_nombre_de_cliente_se_recorta()
    {
        var pedido = new Order(
            Guid.NewGuid(), "  Ana  ", ServiceType.DineIn, [Crepa()], Ahora);

        Assert.Equal("Ana", pedido.CustomerName);
    }

    [Fact]
    public void Nombre_de_cliente_demasiado_largo_es_invalido()
    {
        var nombre = new string('a', Order.MaxCustomerNameLength + 1);

        Assert.Throws<ArgumentException>(
            () => new Order(Guid.NewGuid(), nombre, ServiceType.DineIn, [Crepa()], Ahora));
    }

    [Fact]
    public void Modalidad_inexistente_es_invalida()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Order(Guid.NewGuid(), "Ana", (ServiceType)99, [Crepa()], Ahora));
    }

    [Fact]
    public void Flujo_completo_pasa_por_todos_los_estados_y_guarda_las_horas()
    {
        var pedido = PedidoNuevo();

        pedido.MarkPaid(Ahora.AddMinutes(1));
        Assert.Equal(OrderStatus.Paid, pedido.Status);

        pedido.StartPreparation(Ahora.AddMinutes(2));
        Assert.Equal(OrderStatus.InPreparation, pedido.Status);

        pedido.MarkReady(Ahora.AddMinutes(10));
        Assert.Equal(OrderStatus.Ready, pedido.Status);

        pedido.MarkDelivered(Ahora.AddMinutes(12));
        Assert.Equal(OrderStatus.Delivered, pedido.Status);

        Assert.Equal(Ahora.AddMinutes(1), pedido.PaidAt);
        Assert.Equal(Ahora.AddMinutes(2), pedido.PreparationStartedAt);
        Assert.Equal(Ahora.AddMinutes(10), pedido.ReadyAt);
        Assert.Equal(Ahora.AddMinutes(12), pedido.DeliveredAt);
    }

    [Fact]
    public void No_se_puede_preparar_un_pedido_sin_pagar()
    {
        var pedido = PedidoNuevo();

        Assert.Throws<InvalidOperationException>(() => pedido.StartPreparation(Ahora));
    }

    [Fact]
    public void No_se_puede_pagar_dos_veces()
    {
        var pedido = PedidoNuevo();
        pedido.MarkPaid(Ahora);

        Assert.Throws<InvalidOperationException>(() => pedido.MarkPaid(Ahora));
    }

    [Fact]
    public void No_se_puede_entregar_un_pedido_que_no_esta_listo()
    {
        var pedido = PedidoNuevo();
        pedido.MarkPaid(Ahora);
        pedido.StartPreparation(Ahora);

        Assert.Throws<InvalidOperationException>(() => pedido.MarkDelivered(Ahora));
    }

    [Fact]
    public void Se_puede_cancelar_un_pedido_pendiente_de_pago()
    {
        var pedido = PedidoNuevo();

        pedido.Cancel("  El cliente se arrepintió  ", Ahora.AddMinutes(3));

        Assert.Equal(OrderStatus.Cancelled, pedido.Status);
        Assert.Equal("El cliente se arrepintió", pedido.CancellationReason);
        Assert.Equal(Ahora.AddMinutes(3), pedido.CancelledAt);
    }

    [Fact]
    public void Se_puede_cancelar_un_pedido_pagado_antes_de_prepararlo()
    {
        var pedido = PedidoNuevo();
        pedido.MarkPaid(Ahora);

        pedido.Cancel("Producto agotado", Ahora);

        Assert.Equal(OrderStatus.Cancelled, pedido.Status);
    }

    [Fact]
    public void No_se_puede_cancelar_un_pedido_en_preparacion()
    {
        var pedido = PedidoNuevo();
        pedido.MarkPaid(Ahora);
        pedido.StartPreparation(Ahora);

        Assert.Throws<InvalidOperationException>(() => pedido.Cancel("Tarde", Ahora));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Cancelar_sin_motivo_es_invalido(string motivo)
    {
        var pedido = PedidoNuevo();

        Assert.Throws<ArgumentException>(() => pedido.Cancel(motivo, Ahora));
    }

    [Fact]
    public void Un_pedido_cancelado_no_se_puede_pagar()
    {
        var pedido = PedidoNuevo();
        pedido.Cancel("Error", Ahora);

        Assert.Throws<InvalidOperationException>(() => pedido.MarkPaid(Ahora));
    }
    [Fact]
    public void Motivo_de_cancelacion_demasiado_largo_es_invalido()
    {
        var pedido = PedidoNuevo();
        var motivo = new string('a', Order.MaxCancellationReasonLength + 1);

        Assert.Throws<ArgumentException>(() => pedido.Cancel(motivo, Ahora));
    }
}