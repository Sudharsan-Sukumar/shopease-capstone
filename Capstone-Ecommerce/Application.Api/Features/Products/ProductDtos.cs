using System.ComponentModel.DataAnnotations;

namespace Application.Api.Features.Products;

public record ProductDto(int Id, string Name, string Description, decimal Price, int Stock, string? ImageUrl, string CategoryName, int CategoryId, bool IsActive);

public record CreateProductDto(
    [Required, MaxLength(200)] string Name,
    string Description,
    [Range(0.01, double.MaxValue)] decimal Price,
    [Range(0, int.MaxValue)] int Stock,
    string? ImageUrl,
    [Required] int CategoryId);

public record UpdateProductDto(
    [Required, MaxLength(200)] string Name,
    string Description,
    [Range(0.01, double.MaxValue)] decimal Price,
    [Range(0, int.MaxValue)] int Stock,
    string? ImageUrl,
    [Required] int CategoryId,
    bool IsActive);

public record CategoryDto(int Id, string Name, string Slug);
