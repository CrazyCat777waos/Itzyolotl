using Itzyolotl.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Itzyolotl.Infrastructure.Persistence.Configurations;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("OrderLines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.ProductName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(l => l.UnitPrice)
            .HasPrecision(10, 2);

        builder.Property(l => l.Note)
            .HasMaxLength(OrderLine.MaxNoteLength);

        builder.Ignore(l => l.Subtotal);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}