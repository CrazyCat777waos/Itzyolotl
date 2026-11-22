using Itzyolotl.Application.Menu;

using Microsoft.Extensions.DependencyInjection;

namespace Itzyolotl.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<GetMenuHandler>();

        return services;
    }
}