namespace Application.Api.Features.Content;

/// <summary>
/// A single admin-managed banner/promo block. `Key` is a stable slug
/// (e.g. "homepage-hero") a specific frontend placement can look up by,
/// while `DisplayOrder` controls ordering among all active blocks for
/// placements that just render "whatever's active" (the product catalog
/// banner strip).
/// </summary>
public class ContentBlock
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
