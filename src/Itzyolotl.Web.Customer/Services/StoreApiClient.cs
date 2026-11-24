using System.Net.Http.Json;

using Itzyolotl.Contracts.Store;

namespace Itzyolotl.Web.Customer.Services;

public sealed class StoreApiClient(HttpClient http)
{
    public Task<StoreStatusDto?> GetStatusAsync(CancellationToken cancellationToken = default) =>
        http.GetFromJsonAsync<StoreStatusDto>("api/store/status", cancellationToken);
}