using Itzyolotl.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Itzyolotl.Infrastructure.Persistence.Configurations;

public class StoreSettingsConfiguration : IEntityTypeConfiguration<StoreSettings>
{
    public void Configure(EntityTypeBuilder<StoreSettings> builder)
    {
        var range = $"BETWEEN {StoreSettings.MinCapacity} AND {StoreSettings.MaxCapacity}";

        builder.ToTable("StoreSettings", table =>
        {
            table.HasCheckConstraint(
                "CK_StoreSettings_Singleton",
                $"[Id] = {StoreSettings.SingletonId}");

            table.HasCheckConstraint("CK_StoreSettings_CrepesCapacity", $"[CrepesCapacity] {range}");
            table.HasCheckConstraint("CK_StoreSettings_CoffeeAndPaninisCapacity", $"[CoffeeAndPaninisCapacity] {range}");
            table.HasCheckConstraint("CK_StoreSettings_BackCapacity", $"[BackCapacity] {range}");
        });

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).ValueGeneratedNever();

        // Fila unica sembrada por la migracion (tambien en produccion).
        builder.HasData(new
        {
            Id = StoreSettings.SingletonId,
            CrepesCapacity = StoreSettings.DefaultCapacity,
            CoffeeAndPaninisCapacity = StoreSettings.DefaultCapacity,
            BackCapacity = StoreSettings.DefaultCapacity
        });
    }
}