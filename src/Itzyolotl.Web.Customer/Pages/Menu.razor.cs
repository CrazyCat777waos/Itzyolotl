using Itzyolotl.Contracts.Menu;
using Itzyolotl.Web.Customer.Services;

using Microsoft.AspNetCore.Components;

namespace Itzyolotl.Web.Customer.Pages;

public partial class Menu
{
    [Inject]
    private MenuApiClient MenuApi { get; set; } = default!;

    private MenuDto? menu;
    private string? error;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            menu = await MenuApi.GetMenuAsync();
        }
        catch (HttpRequestException)
        {
            error = "No se pudo cargar el menú. Revisa tu conexión e inténtalo de nuevo.";
        }
    }
}