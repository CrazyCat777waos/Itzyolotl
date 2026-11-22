using Itzyolotl.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Itzyolotl.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<OpeningPeriod> OpeningPeriods { get; }
}