using System.ComponentModel.DataAnnotations;

namespace Application.Api.Features.Cart;

public record CartItemDto(int ProductId, string ProductName, decimal UnitPrice, int Quantity, int AvailableStock);

public record CartDto(List<CartItemDto> Items, decimal Total);

public record AddCartItemDto([Required] int ProductId, [Range(1, int.MaxValue)] int Quantity);

public record UpdateCartItemDto([Range(1, int.MaxValue)] int Quantity);
