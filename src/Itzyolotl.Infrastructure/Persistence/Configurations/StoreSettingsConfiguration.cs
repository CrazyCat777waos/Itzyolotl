using Itzyolotl.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Itzyolotl.Infrastructure.Persistence.Configurations;

public class StoreSettingsConfiguration : IEntityTypeConfiguration<StoreSettings>
{
    public void Configure(EntityTypeBuilder<StoreSettings> builder)
    {
        builder.ToTable("StoreSettings", table =>
        {
            table.HasCheckConstraint(
                "CK_StoreSettings_Singleton",
                $"[Id] = {StoreSettings.SingletonId}");

            table.HasCheckConstraint(
                "CK_StoreSettings_Capacity",
                $"[Capacity] BETWEEN {StoreSettings.MinCapacity} AND {StoreSettings.MaxCapacity}");
        });

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).ValueGeneratedNever();

        // Fila unica sembrada por la migracion (tambien en produccion).
        builder.HasData(new
        {
            Id = StoreSettings.SingletonId,
            Capacity = StoreSettings.DefaultCapacity
        });
    }
}