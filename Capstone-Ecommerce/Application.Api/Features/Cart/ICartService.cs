using Application.Api.Common.Results;

namespace Application.Api.Features.Cart;

public interface ICartService
{
    Task<Result<CartDto>> GetCartAsync(int userId, CancellationToken ct = default);
    Task<Result<CartDto>> AddItemAsync(int userId, AddCartItemDto dto, CancellationToken ct = default);
    Task<Result<CartDto>> UpdateItemAsync(int userId, int productId, UpdateCartItemDto dto, CancellationToken ct = default);
    Task<Result<CartDto>> RemoveItemAsync(int userId, int productId, CancellationToken ct = default);
    Task<Result> ClearCartAsync(int userId, CancellationToken ct = default);
}
