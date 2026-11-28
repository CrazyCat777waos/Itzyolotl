using Itzyolotl.Domain.Entities;
using Itzyolotl.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Itzyolotl.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<OpeningPeriod> OpeningPeriods => Set<OpeningPeriod>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<StoreSettings> Settings => Set<StoreSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}