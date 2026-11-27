using Itzyolotl.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Itzyolotl.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerName)
            .IsRequired()
            .HasMaxLength(Order.MaxCustomerNameLength);

        builder.Property(o => o.CancellationReason)
            .HasMaxLength(Order.MaxCancellationReasonLength);

        builder.Property(o => o.RowVersion).IsRowVersion();

        builder.Ignore(o => o.Total);

        builder.HasMany(o => o.Lines)
            .WithOne()
            .HasForeignKey("OrderId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Lines)
            .HasField("lines")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(o => o.IdempotencyKey).IsUnique();
        builder.HasIndex(o => new { o.Status, o.CreatedAt });
    }
}