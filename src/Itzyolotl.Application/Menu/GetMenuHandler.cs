using Itzyolotl.Application.Abstractions;
using Itzyolotl.Contracts.Menu;

using Microsoft.EntityFrameworkCore;

namespace Itzyolotl.Application.Menu;

public sealed class GetMenuHandler(IAppDbContext db)
{
    public async Task<MenuDto> HandleAsync(CancellationToken cancellationToken)
    {
        var rows = await (
            from c in db.Categories.AsNoTracking()
            join p in db.Products.AsNoTracking() on c.Id equals p.CategoryId
            orderby c.DisplayOrder, p.Name
            select new
            {
                CategoryId = c.Id,
                CategoryName = c.Name,
                ProductId = p.Id,
                ProductName = p.Name,
                p.Description,
                p.Price,
                p.PreparationMinutes,
                p.IsAvailable
            })
            .ToListAsync(cancellationToken);

        var categories = rows
            .GroupBy(r => new { r.CategoryId, r.CategoryName })
            .Select(g => new MenuCategoryDto(
                g.Key.CategoryId,
                g.Key.CategoryName,
                g.Select(r => new MenuProductDto(
                    r.ProductId,
                    r.ProductName,
                    r.Description,
                    r.Price,
                    r.PreparationMinutes,
                    r.IsAvailable)).ToList()))
            .ToList();

        return new MenuDto(categories);
    }
}