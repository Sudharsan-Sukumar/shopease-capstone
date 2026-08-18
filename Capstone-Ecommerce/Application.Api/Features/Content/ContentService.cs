using Application.Api.Common.Repository;
using Application.Api.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Features.Content;

public class ContentService(IUnitOfWork uow) : IContentService
{
    public async Task<Result<List<ContentBlockDto>>> GetActiveAsync(CancellationToken ct = default)
    {
        var blocks = await uow.Repository<ContentBlock>().Query()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => ToDto(c))
            .ToListAsync(ct);

        return Result.Success(blocks);
    }

    public async Task<Result<List<ContentBlockDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var blocks = await uow.Repository<ContentBlock>().Query()
            .OrderBy(c => c.DisplayOrder)
            .Select(c => ToDto(c))
            .ToListAsync(ct);

        return Result.Success(blocks);
    }

    public async Task<Result<ContentBlockDto>> CreateAsync(CreateContentBlockDto dto, CancellationToken ct = default)
    {
        var keyTaken = await uow.Repository<ContentBlock>().Query().AnyAsync(c => c.Key == dto.Key, ct);
        if (keyTaken)
            return Error.Conflict("Content.KeyTaken", $"A content block with key \"{dto.Key}\" already exists.");

        var block = new ContentBlock
        {
            Key = dto.Key,
            Title = dto.Title,
            Body = dto.Body,
            ImageUrl = dto.ImageUrl,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder,
        };

        await uow.Repository<ContentBlock>().AddAsync(block, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(ToDto(block));
    }

    public async Task<Result<ContentBlockDto>> UpdateAsync(int id, UpdateContentBlockDto dto, CancellationToken ct = default)
    {
        var block = await uow.Repository<ContentBlock>().GetByIdAsync(id, ct);
        if (block is null)
            return Error.NotFound("Content.NotFound", $"Content block {id} was not found.");

        block.Title = dto.Title;
        block.Body = dto.Body;
        block.ImageUrl = dto.ImageUrl;
        block.IsActive = dto.IsActive;
        block.DisplayOrder = dto.DisplayOrder;
        block.UpdatedAtUtc = DateTime.UtcNow;

        await uow.SaveChangesAsync(ct);

        return Result.Success(ToDto(block));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var block = await uow.Repository<ContentBlock>().GetByIdAsync(id, ct);
        if (block is null)
            return Error.NotFound("Content.NotFound", $"Content block {id} was not found.");

        // A real delete, not a soft one - unlike Product, nothing else
        // (like an order line item) ever references a ContentBlock by id.
        uow.Repository<ContentBlock>().Remove(block);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static ContentBlockDto ToDto(ContentBlock c) =>
        new(c.Id, c.Key, c.Title, c.Body, c.ImageUrl, c.IsActive, c.DisplayOrder, c.CreatedAtUtc, c.UpdatedAtUtc);
}
