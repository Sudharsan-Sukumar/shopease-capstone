namespace Application.Api.Features.Orders;

public record OrderItemDto(int ProductId, string ProductName, decimal UnitPrice, int Quantity);

public record OrderDto(
    int Id,
    string Status,
    decimal Total,
    DateTime CreatedAtUtc,
    string? PaymentTransactionId,
    List<OrderItemDto> Items);
