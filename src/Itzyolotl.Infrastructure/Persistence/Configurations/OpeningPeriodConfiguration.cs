using Itzyolotl.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Itzyolotl.Infrastructure.Persistence.Configurations;

public class OpeningPeriodConfiguration : IEntityTypeConfiguration<OpeningPeriod>
{
    public void Configure(EntityTypeBuilder<OpeningPeriod> builder)
    {
        builder.ToTable("OpeningPeriods");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OpensAt).HasColumnType("time(0)");
        builder.Property(o => o.ClosesAt).HasColumnType("time(0)");

        builder.HasIndex(o => new { o.Day, o.OpensAt }).IsUnique();
    }
}