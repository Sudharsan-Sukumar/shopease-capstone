using Application.Api.Common.Results;

namespace Application.Api.Features.Orders;

public interface IOrderService
{
    Task<Result<OrderDto>> CheckoutAsync(int userId, CancellationToken ct = default);
    Task<Result<List<OrderDto>>> GetMyOrdersAsync(int userId, CancellationToken ct = default);
    Task<Result<OrderDto>> GetMyOrderByIdAsync(int userId, int orderId, CancellationToken ct = default);

    /// <summary>Admin-only capability - enforced at the controller via [Authorize(Roles="Admin")], not here.</summary>
    Task<Result<List<OrderDto>>> GetAllOrdersAsync(CancellationToken ct = default);
}
