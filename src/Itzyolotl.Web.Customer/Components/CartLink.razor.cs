using Itzyolotl.Web.Customer.Services;

using Microsoft.AspNetCore.Components;

namespace Itzyolotl.Web.Customer.Components;

public partial class CartLink : IDisposable
{
    [Inject]
    private CartState Cart { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Cart.Changed += OnCartChanged;
        await Cart.EnsureLoadedAsync();
    }

    private void OnCartChanged() => _ = InvokeAsync(StateHasChanged);

    public void Dispose() => Cart.Changed -= OnCartChanged;
}