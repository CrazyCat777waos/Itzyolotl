using Itzyolotl.Domain.Entities;
using Itzyolotl.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Itzyolotl.Infrastructure;

public static class DevelopmentDataSeeder
{
    public static async Task SeedDevelopmentDataAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedMenuAsync(db);
        await SeedOpeningPeriodsAsync(db);
    }

    private static async Task SeedMenuAsync(AppDbContext db)
    {
        if (await db.Categories.AnyAsync())
        {
            return;
        }

        var cafes = new Category("Cafés", 1);
        var crepasSaladas = new Category("Crepas saladas", 2);
        var crepasDulces = new Category("Crepas dulces", 3);
        var frappes = new Category("Frappes", 4);
        var paninis = new Category("Paninis", 5);

        db.Categories.AddRange(cafes, crepasSaladas, crepasDulces, frappes, paninis);
        await db.SaveChangesAsync();

        db.Products.AddRange(
            new Product(cafes.Id, "Capuchino", 45m, 4, "Espresso con leche vaporizada y espuma."),
            new Product(cafes.Id, "Americano", 35m, 3),
            new Product(cafes.Id, "Latte", 48m, 4),
            new Product(crepasSaladas.Id, "Crepa de chorizo argentino", 85m, 10, "Con aceitunas y queso crema."),
            new Product(crepasSaladas.Id, "Crepa de jamón y queso", 75m, 9),
            new Product(crepasDulces.Id, "Crepa de cajeta", 65m, 8),
            new Product(crepasDulces.Id, "Crepa de fresa con crema", 70m, 8),
            new Product(frappes.Id, "Frappe de vainilla", 60m, 6),
            new Product(frappes.Id, "Frappe de moka", 65m, 6),
            new Product(paninis.Id, "Panini de pollo", 80m, 8));

        await db.SaveChangesAsync();
    }

    private static async Task SeedOpeningPeriodsAsync(AppDbContext db)
    {
        if (await db.OpeningPeriods.AnyAsync())
        {
            return;
        }

        var opensAt = new TimeOnly(9, 0);
        var closesAt = new TimeOnly(20, 0);

        DayOfWeek[] openDays =
        [
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday
        ];

        db.OpeningPeriods.AddRange(
            openDays.Select(day => new OpeningPeriod(day, opensAt, closesAt)));

        await db.SaveChangesAsync();
    }
}