using Application.Api.Common.Results;

namespace Application.Api.Features.Content;

public interface IContentService
{
    /// <summary>Public - only active blocks, ordered for display.</summary>
    Task<Result<List<ContentBlockDto>>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>Staff - everything, including inactive/draft blocks.</summary>
    Task<Result<List<ContentBlockDto>>> GetAllAsync(CancellationToken ct = default);

    Task<Result<ContentBlockDto>> CreateAsync(CreateContentBlockDto dto, CancellationToken ct = default);
    Task<Result<ContentBlockDto>> UpdateAsync(int id, UpdateContentBlockDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
