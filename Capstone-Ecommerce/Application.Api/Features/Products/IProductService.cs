using Application.Api.Common.Results;

namespace Application.Api.Features.Products;

public interface IProductService
{
    Task<Result<List<ProductDto>>> GetAllAsync(string? search = null, CancellationToken ct = default);

    /// <summary>Staff-only - includes inactive (soft-deleted) products so they can be found and reactivated.</summary>
    Task<Result<List<ProductDto>>> GetAllForAdminAsync(CancellationToken ct = default);
    Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default);
    Task<Result<ProductDto>> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
    Task<Result<List<CategoryDto>>> GetCategoriesAsync(CancellationToken ct = default);
}
