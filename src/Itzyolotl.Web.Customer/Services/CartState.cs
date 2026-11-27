using System.Text.Json;

using Itzyolotl.Contracts.Menu;
using Itzyolotl.Contracts.Orders;

using Microsoft.JSInterop;

namespace Itzyolotl.Web.Customer.Services;

public sealed class CartState(IJSRuntime js)
{
    private const string CartKey = "itzyolotl.cart";
    private const string LastOrderKey = "itzyolotl.last-order";

    private readonly List<CartItem> items = [];
    private Guid? pendingKey;
    private Task? loading;

    public event Action? Changed;

    public IReadOnlyList<CartItem> Items => items;

    public int Count => items.Sum(item => item.Quantity);

    public decimal Total => items.Sum(item => item.Subtotal);

    public string CustomerName { get; private set; } = string.Empty;

    public ServiceTypeDto ServiceType { get; private set; } = ServiceTypeDto.DineIn;

    public Guid? LastOrderId { get; private set; }

    public Task EnsureLoadedAsync() => loading ??= LoadAsync();

    public int QuantityOf(int productId) =>
        items.FirstOrDefault(item => item.ProductId == productId)?.Quantity ?? 0;

    public async Task AddAsync(MenuProductDto product)
    {
        await EnsureLoadedAsync();

        var index = items.FindIndex(item => item.ProductId == product.Id);

        if (index >= 0)
        {
            var current = items[index];
            var quantity = Math.Min(current.Quantity + 1, OrderLimits.MaxQuantityPerLine);
            items[index] = current with { Quantity = quantity };
        }
        else if (items.Count < OrderLimits.MaxLines)
        {
            items.Add(new CartItem(product.Id, product.Name, product.Price, 1, null));
        }

        await CartChangedAsync();
    }

    public async Task SetQuantityAsync(int productId, int quantity)
    {
        var index = items.FindIndex(item => item.ProductId == productId);

        if (index < 0)
        {
            return;
        }

        if (quantity < 1)
        {
            items.RemoveAt(index);
        }
        else
        {
            var clamped = Math.Min(quantity, OrderLimits.MaxQuantityPerLine);
            items[index] = items[index] with { Quantity = clamped };
        }

        await CartChangedAsync();
    }

    public async Task SetNoteAsync(int productId, string? note)
    {
        var index = items.FindIndex(item => item.ProductId == productId);

        if (index < 0)
        {
            return;
        }

        var cleaned = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        items[index] = items[index] with { Note = cleaned };

        await CartChangedAsync();
    }

    public async Task<CreateOrderRequest> PrepareRequestAsync(
        string customerName,
        ServiceTypeDto serviceType)
    {
        CustomerName = customerName.Trim();
        ServiceType = serviceType;
        pendingKey ??= Guid.NewGuid();

        await PersistCartAsync();

        return new CreateOrderRequest(
            pendingKey.Value,
            CustomerName,
            ServiceType,
            items
                .Select(item => new CreateOrderLineRequest(item.ProductId, item.Quantity, item.Note))
                .ToList());
    }

    public async Task CompleteOrderAsync(Guid orderId)
    {
        items.Clear();
        pendingKey = null;
        LastOrderId = orderId;

        await PersistCartAsync();
        await SafeSetItemAsync(LastOrderKey, orderId.ToString());

        Changed?.Invoke();
    }

    private async Task CartChangedAsync()
    {
        pendingKey = null;
        await PersistCartAsync();
        Changed?.Invoke();
    }

    private async Task LoadAsync()
    {
        try
        {
            var cartJson = await js.InvokeAsync<string?>("localStorage.getItem", CartKey);

            if (cartJson is not null
                && JsonSerializer.Deserialize<CartSnapshot>(cartJson) is { } snapshot)
            {
                items.AddRange(snapshot.Items.Where(IsValid));
                pendingKey = snapshot.PendingKey;
                CustomerName = snapshot.CustomerName ?? string.Empty;
                ServiceType = Enum.IsDefined(snapshot.ServiceType)
                    ? snapshot.ServiceType
                    : ServiceTypeDto.DineIn;
            }

            var lastOrder = await js.InvokeAsync<string?>("localStorage.getItem", LastOrderKey);

            if (Guid.TryParse(lastOrder, out var lastOrderId))
            {
                LastOrderId = lastOrderId;
            }
        }
        catch (Exception ex) when (ex is JSException or JsonException)
        {
            // Sin almacenamiento disponible o con datos corruptos: se empieza con el carrito vacio.
        }

        Changed?.Invoke();
    }

    private static bool IsValid(CartItem item) =>
        item is { ProductId: > 0, UnitPrice: > 0 }
        && item.Quantity is >= 1 and <= OrderLimits.MaxQuantityPerLine
        && !string.IsNullOrWhiteSpace(item.Name);

    private Task PersistCartAsync()
    {
        var snapshot = new CartSnapshot([.. items], pendingKey, CustomerName, ServiceType);

        return SafeSetItemAsync(CartKey, JsonSerializer.Serialize(snapshot));
    }

    private async Task SafeSetItemAsync(string key, string value)
    {
        try
        {
            await js.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        catch (JSException)
        {
            // Si el navegador no deja guardar, el carrito sigue funcionando en memoria.
        }
    }

    private sealed record CartSnapshot(
        List<CartItem> Items,
        Guid? PendingKey,
        string? CustomerName,
        ServiceTypeDto ServiceType);
}