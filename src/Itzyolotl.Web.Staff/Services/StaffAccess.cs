using Microsoft.JSInterop;

namespace Itzyolotl.Web.Staff.Services;

// TEMPORAL: la clave del personal se reemplaza por login con Identity en la Fase 4.
public sealed class StaffAccess(IJSRuntime js)
{
    private const string StorageKey = "itzyolotl.staff-key";

    private Task? loading;

    public event Action? Changed;

    public string? Key { get; private set; }

    public bool HasKey => !string.IsNullOrEmpty(Key);

    public string? Message { get; private set; }

    public Task EnsureLoadedAsync() => loading ??= LoadAsync();

    public async Task SignInAsync(string key)
    {
        Key = key.Trim();
        Message = null;

        await SaveAsync(Key);

        Changed?.Invoke();
    }

    public async Task SignOutAsync(string? message = null)
    {
        Key = null;
        Message = message;

        await SaveAsync(null);

        Changed?.Invoke();
    }

    private async Task LoadAsync()
    {
        try
        {
            Key = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        }
        catch (JSException)
        {
            // Sin almacenamiento disponible: se pedira la clave en cada visita.
        }

        Changed?.Invoke();
    }

    private async Task SaveAsync(string? key)
    {
        try
        {
            if (key is null)
            {
                await js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
            }
            else
            {
                await js.InvokeVoidAsync("localStorage.setItem", StorageKey, key);
            }
        }
        catch (JSException)
        {
            // Si no se puede guardar, la sesion sigue funcionando mientras la pestana este abierta.
        }
    }
}