using Application.Api.Common.Results;
using Application.Api.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Features.Cart;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController(ICartService cartService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        this.ToApiResponse(await cartService.GetCartAsync(User.GetUserId(), ct));

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddCartItemDto dto, CancellationToken ct) =>
        this.ToApiResponse(await cartService.AddItemAsync(User.GetUserId(), dto, ct));

    [HttpPut("items/{productId:int}")]
    public async Task<IActionResult> UpdateItem(int productId, UpdateCartItemDto dto, CancellationToken ct) =>
        this.ToApiResponse(await cartService.UpdateItemAsync(User.GetUserId(), productId, dto, ct));

    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> RemoveItem(int productId, CancellationToken ct) =>
        this.ToApiResponse(await cartService.RemoveItemAsync(User.GetUserId(), productId, ct));

    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken ct) =>
        this.ToApiResponse(await cartService.ClearCartAsync(User.GetUserId(), ct));
}
