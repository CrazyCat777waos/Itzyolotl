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
        var chocolates = new Category("Chocolates", 6);
        var sodas = new Category("Sodas y refrescos", 7);

        db.Categories.AddRange(cafes, crepasSaladas, crepasDulces, frappes, paninis, chocolates, sodas);
        await db.SaveChangesAsync();

        const PreparationStation crepes = PreparationStation.Crepes;
        const PreparationStation cafe = PreparationStation.CoffeeAndPaninis;
        const PreparationStation atras = PreparationStation.Back;
        const PreparationStation sinPreparacion = PreparationStation.None;

        db.Products.AddRange(
            new Product(cafes.Id, "Capuchino", 45m, cafe, 4, "Espresso con leche vaporizada y espuma."),
            new Product(cafes.Id, "Americano", 35m, cafe, 3),
            new Product(cafes.Id, "Latte", 48m, cafe, 4),
            new Product(crepasSaladas.Id, "Crepa de chorizo argentino", 85m, crepes, 10, "Con aceitunas y queso crema."),
            new Product(crepasSaladas.Id, "Crepa de jamón y queso", 75m, crepes, 9),
            new Product(crepasDulces.Id, "Crepa de cajeta", 65m, crepes, 8),
            new Product(crepasDulces.Id, "Crepa de fresa con crema", 70m, crepes, 8),
            new Product(frappes.Id, "Frappe de vainilla", 60m, atras, 6),
            new Product(frappes.Id, "Frappe de moka", 65m, atras, 6),
            new Product(paninis.Id, "Panini de pollo", 80m, cafe, 8),
            new Product(chocolates.Id, "Chocolate caliente", 50m, cafe, 4),
            new Product(chocolates.Id, "Chocolate frío", 55m, atras, 5),
            new Product(sodas.Id, "Soda italiana de fresa", 45m, atras, 4),
            new Product(sodas.Id, "Coca-Cola (lata)", 30m, sinPreparacion, 0));

        await db.SaveChangesAsync();
    }

    private static async Task SeedOpeningPeriodsAsync(AppDbContext db)
    {
        if (await db.OpeningPeriods.AnyAsync())
        {
            return;
        }

        var opensAt = new TimeOnly(19, 0);
        var closesAt = new TimeOnly(23, 0);

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