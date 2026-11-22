using Itzyolotl.Application.Menu;
using Itzyolotl.Application.Store;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Itzyolotl.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<GetMenuHandler>();
        services.AddScoped<GetStoreStatusHandler>();

        return services;
    }
}