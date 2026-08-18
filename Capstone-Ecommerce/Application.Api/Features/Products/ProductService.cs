using Application.Api.Common.Repository;
using Application.Api.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Features.Products;

public class ProductService(IUnitOfWork uow) : IProductService
{
    public async Task<Result<List<ProductDto>>> GetAllAsync(string? search = null, CancellationToken ct = default)
    {
        var query = uow.Repository<Product>().Query()
            .Include(p => p.Category)
            .Where(p => p.IsActive);

        // Server-side filter, not client-side - this is what makes the
        // Angular search worth debouncing/switchMap-ing in the first place:
        // every keystroke that survives the debounce is a real round trip.
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search));

        var products = await query.Select(p => ToDto(p)).ToListAsync(ct);

        return Result.Success(products);
    }

    public async Task<Result<List<ProductDto>>> GetAllForAdminAsync(CancellationToken ct = default)
    {
        var products = await uow.Repository<Product>().Query()
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .Select(p => ToDto(p))
            .ToListAsync(ct);

        return Result.Success(products);
    }

    public async Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var product = await uow.Repository<Product>().Query()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null)
            return Error.NotFound("Product.NotFound", $"Product {id} was not found.");

        return Result.Success(ToDto(product));
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        var categoryExists = await uow.Repository<Category>().Query().AnyAsync(c => c.Id == dto.CategoryId, ct);
        if (!categoryExists)
            return Error.Validation("Product.InvalidCategory", $"Category {dto.CategoryId} does not exist.");

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            ImageUrl = dto.ImageUrl,
            CategoryId = dto.CategoryId,
        };

        await uow.Repository<Product>().AddAsync(product, ct);
        await uow.SaveChangesAsync(ct);

        return await GetByIdAsync(product.Id, ct);
    }

    public async Task<Result<ProductDto>> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct = default)
    {
        var product = await uow.Repository<Product>().GetByIdAsync(id, ct);
        if (product is null)
            return Error.NotFound("Product.NotFound", $"Product {id} was not found.");

        var categoryExists = await uow.Repository<Category>().Query().AnyAsync(c => c.Id == dto.CategoryId, ct);
        if (!categoryExists)
            return Error.Validation("Product.InvalidCategory", $"Category {dto.CategoryId} does not exist.");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.ImageUrl = dto.ImageUrl;
        product.CategoryId = dto.CategoryId;
        product.IsActive = dto.IsActive;

        await uow.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var product = await uow.Repository<Product>().GetByIdAsync(id, ct);
        if (product is null)
            return Error.NotFound("Product.NotFound", $"Product {id} was not found.");

        // Soft delete - keeps past order line items pointing at a real row.
        product.IsActive = false;
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<List<CategoryDto>>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var categories = await uow.Repository<Category>().Query()
            .Select(c => new CategoryDto(c.Id, c.Name, c.Slug))
            .ToListAsync(ct);

        return Result.Success(categories);
    }

    private static ProductDto ToDto(Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.Stock, p.ImageUrl, p.Category.Name, p.CategoryId, p.IsActive);
}
