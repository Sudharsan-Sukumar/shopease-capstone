using System.ComponentModel.DataAnnotations;

namespace Application.Api.Features.Content;

public record ContentBlockDto(
    int Id, string Key, string Title, string Body, string? ImageUrl,
    bool IsActive, int DisplayOrder, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);

public record CreateContentBlockDto(
    [Required, StringLength(100, MinimumLength = 2), RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Key must be lowercase letters, digits, and hyphens only.")]
    string Key,
    [Required, StringLength(200, MinimumLength = 2)] string Title,
    [Required, StringLength(1000, MinimumLength = 2)] string Body,
    string? ImageUrl,
    bool IsActive,
    int DisplayOrder);

public record UpdateContentBlockDto(
    [Required, StringLength(200, MinimumLength = 2)] string Title,
    [Required, StringLength(1000, MinimumLength = 2)] string Body,
    string? ImageUrl,
    bool IsActive,
    int DisplayOrder);
