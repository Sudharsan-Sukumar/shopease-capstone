using Application.Api.Features.Auth;

namespace Application.Api.Features.Orders;

public enum OrderStatus
{
    Confirmed,
    PaymentPending,
}

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string? PaymentTransactionId { get; set; }
    public int PaymentAttempts { get; set; }

    public List<OrderItem> Items { get; set; } = [];
}
