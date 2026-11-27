using Itzyolotl.Contracts.Orders;
using Itzyolotl.Web.Customer.Services;

using Microsoft.AspNetCore.Components;

namespace Itzyolotl.Web.Customer.Pages;

public partial class ShoppingCart
{
    [Inject]
    private CartState Cart { get; set; } = default!;

    [Inject]
    private OrdersApiClient OrdersApi { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private bool loaded;
    private bool sending;
    private string? error;
    private string customerName = string.Empty;
    private ServiceTypeDto serviceType = ServiceTypeDto.DineIn;

    protected override async Task OnInitializedAsync()
    {
        await Cart.EnsureLoadedAsync();

        customerName = Cart.CustomerName;
        serviceType = Cart.ServiceType;
        loaded = true;
    }

    private Task OnNoteChanged(int productId, ChangeEventArgs e) =>
        Cart.SetNoteAsync(productId, e.Value?.ToString());

    private string OptionClass(ServiceTypeDto option) =>
        option == serviceType
            ? "rounded-xl border border-marca bg-marca/10 py-3 font-bold text-marca"
            : "rounded-xl border border-borde bg-superficie py-3 font-bold";

    private async Task SendAsync()
    {
        error = null;

        if (string.IsNullOrWhiteSpace(customerName))
        {
            error = "Escribe tu nombre para el pedido.";
            return;
        }

        sending = true;

        try
        {
            var request = await Cart.PrepareRequestAsync(customerName, serviceType);
            var result = await OrdersApi.CreateAsync(request);

            if (result.Order is null)
            {
                error = result.Error;
                return;
            }

            await Cart.CompleteOrderAsync(result.Order.Id);
            Navigation.NavigateTo($"/pedido/{result.Order.Id}");
        }
        catch (HttpRequestException)
        {
            error = "No se pudo enviar. Revisa tu conexión e inténtalo de nuevo: tu pedido no se duplicará.";
        }
        finally
        {
            sending = false;
        }
    }
}