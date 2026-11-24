using System.Net.Http.Json;

using Itzyolotl.Contracts.Menu;

namespace Itzyolotl.Web.Customer.Services;

public sealed class MenuApiClient(HttpClient http)
{
    public Task<MenuDto?> GetMenuAsync(CancellationToken cancellationToken = default) =>
        http.GetFromJsonAsync<MenuDto>("api/menu", cancellationToken);
}