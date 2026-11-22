using Itzyolotl.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Itzyolotl.Application.Abstractions;

namespace Itzyolotl.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddSqlServerDbContext<AppDbContext>("itzyolotl");
        builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        return builder;
    }

    public static async Task ApplyMigrationsAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();
    }
}