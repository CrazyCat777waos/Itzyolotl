using System.Net;
using System.Net.Http.Json;

using Itzyolotl.Contracts.Orders;

namespace Itzyolotl.Web.Customer.Services;

public sealed record PlaceOrderResult(OrderDto? Order, string? Error);

public sealed class OrdersApiClient(HttpClient http)
{
    private sealed record ProblemResponse(string? Detail);

    public async Task<PlaceOrderResult> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("api/orders", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var order = await response.Content.ReadFromJsonAsync<OrderDto>(cancellationToken);

            return new PlaceOrderResult(order, null);
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>(cancellationToken);

            return new PlaceOrderResult(null, problem?.Detail ?? "El pedido no es válido.");
        }

        return new PlaceOrderResult(null, "No se pudo enviar el pedido. Inténtalo de nuevo.");
    }

    public async Task<OrderDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await http.GetAsync($"api/orders/{id}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<OrderDto>(cancellationToken);
    }
}