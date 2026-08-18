using Application.Api.Features.Products;

namespace Application.Api.Features.Orders;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // Snapshotted at time of order - the product's live Name/Price can
    // change later (admin edits, Phase 4) without rewriting order history.
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
