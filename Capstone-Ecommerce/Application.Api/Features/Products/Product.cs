using System.ComponentModel.DataAnnotations;

namespace Application.Api.Features.Products;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    /// <summary>Optimistic concurrency token - used by Phase 2's checkout to stop two simultaneous last-unit purchases from overselling.</summary>
    [Timestamp]
    public byte[] RowVersion { get; set; } = [];
}
