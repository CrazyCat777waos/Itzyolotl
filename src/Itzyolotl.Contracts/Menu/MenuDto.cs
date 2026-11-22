namespace Itzyolotl.Contracts.Menu;

public sealed record MenuProductDto(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int PreparationMinutes,
    bool IsAvailable);

public sealed record MenuCategoryDto(
    int Id,
    string Name,
    IReadOnlyList<MenuProductDto> Products);

public sealed record MenuDto(IReadOnlyList<MenuCategoryDto> Categories);