using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Itzyolotl.Contracts.Orders;

namespace Itzyolotl.Web.Staff.Services;

public sealed record StaffActionResult(bool Succeeded, string? Error);

public sealed class StaffApiClient(HttpClient http, StaffAccess access)
{
    private const string KeyHeader = "X-Staff-Key";

    private sealed record ProblemResponse(string? Detail);

    public async Task<IReadOnlyList<OrderDto>> GetQueueAsync(CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, "api/staff/orders");
        using var response = await http.SendAsync(request, cancellationToken);

        EnsureAuthorized(response);
        response.EnsureSuccessStatusCode();

        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>(cancellationToken);

        return orders ?? [];
    }

    public async Task<StaffActionResult> ApplyActionAsync(
        Guid id,
        OrderAction action,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, $"api/staff/orders/{id}/actions");
        request.Content = JsonContent.Create(new OrderActionRequest(action, reason));

        using var response = await http.SendAsync(request, cancellationToken);

        EnsureAuthorized(response);

        if (response.IsSuccessStatusCode)
        {
            return new StaffActionResult(true, null);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new StaffActionResult(false, "El pedido ya no existe.");
        }

        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>(cancellationToken);

            return new StaffActionResult(false, problem?.Detail ?? "No se pudo completar la acción.");
        }
        catch (JsonException)
        {
            return new StaffActionResult(false, "No se pudo completar la acción.");
        }
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string uri)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Add(KeyHeader, access.Key ?? string.Empty);

        return request;
    }

    private static void EnsureAuthorized(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException();
        }
    }
}