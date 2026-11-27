using Itzyolotl.Contracts.Orders;
using Itzyolotl.Domain.Entities;

namespace Itzyolotl.Domain.Tests.Orders;

public class OrderLimitsTests
{
    [Fact]
    public void Los_limites_del_contrato_coinciden_con_los_del_dominio()
    {
        Assert.Equal(Order.MaxCustomerNameLength, OrderLimits.MaxCustomerNameLength);
        Assert.Equal(OrderLine.MaxNoteLength, OrderLimits.MaxNoteLength);
    }
}