using Application.Api.Common.Repository;
using Application.Api.Common.Results;
using Application.Api.Features.Products;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Features.Cart;

public class CartService(IUnitOfWork uow) : ICartService
{
    public async Task<Result<CartDto>> GetCartAsync(int userId, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(userId, ct);
        return Result.Success(ToDto(cart));
    }

    public async Task<Result<CartDto>> AddItemAsync(int userId, AddCartItemDto dto, CancellationToken ct = default)
    {
        var product = await uow.Repository<Product>().Query().FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.IsActive, ct);
        if (product is null)
            return Error.NotFound("Cart.ProductNotFound", $"Product {dto.ProductId} was not found.");

        var cart = await GetOrCreateCartAsync(userId, ct);
        var existing = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
        var newQuantity = (existing?.Quantity ?? 0) + dto.Quantity;

        if (newQuantity > product.Stock)
            return Error.Validation("Cart.InsufficientStock", $"Only {product.Stock} of \"{product.Name}\" available.");

        if (existing is not null)
        {
            existing.Quantity = newQuantity;
        }
        else
        {
            cart.Items.Add(new CartItem { ProductId = product.Id, Quantity = dto.Quantity });
        }

        await uow.SaveChangesAsync(ct);
        return await GetCartAsync(userId, ct);
    }

    public async Task<Result<CartDto>> UpdateItemAsync(int userId, int productId, UpdateCartItemDto dto, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(userId, ct);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            return Error.NotFound("Cart.ItemNotFound", "That item is not in your cart.");

        var product = await uow.Repository<Product>().GetByIdAsync(productId, ct);
        if (product is null || dto.Quantity > product.Stock)
            return Error.Validation("Cart.InsufficientStock", $"Only {product?.Stock ?? 0} available.");

        item.Quantity = dto.Quantity;
        await uow.SaveChangesAsync(ct);
        return await GetCartAsync(userId, ct);
    }

    public async Task<Result<CartDto>> RemoveItemAsync(int userId, int productId, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(userId, ct);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
        {
            uow.Repository<CartItem>().Remove(item);
            await uow.SaveChangesAsync(ct);
        }

        return await GetCartAsync(userId, ct);
    }

    public async Task<Result> ClearCartAsync(int userId, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(userId, ct);
        foreach (var item in cart.Items.ToList())
            uow.Repository<CartItem>().Remove(item);

        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    /// <summary>
    /// Every cart lookup goes through this - the cart is ALWAYS derived from
    /// the authenticated user's id (JWT claim, set by the controller), never
    /// from a route/body parameter, so one user can never read or mutate
    /// another user's cart (no IDOR).
    /// </summary>
    private async Task<Cart> GetOrCreateCartAsync(int userId, CancellationToken ct)
    {
        var cart = await uow.Repository<Cart>().Query()
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (cart is not null) return cart;

        cart = new Cart { UserId = userId };
        await uow.Repository<Cart>().AddAsync(cart, ct);
        await uow.SaveChangesAsync(ct);
        return cart;
    }

    private static CartDto ToDto(Cart cart)
    {
        var items = cart.Items
            .Select(i => new CartItemDto(i.ProductId, i.Product.Name, i.Product.Price, i.Quantity, i.Product.Stock))
            .ToList();

        return new CartDto(items, items.Sum(i => i.UnitPrice * i.Quantity));
    }
}
