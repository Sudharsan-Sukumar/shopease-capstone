using Application.Api.Common.Results;
using Application.Api.Common.Security;
using Application.Api.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Features.Orders;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CancellationToken ct) =>
        this.ToApiResponse(await orderService.CheckoutAsync(User.GetUserId(), ct));

    [HttpGet]
    public async Task<IActionResult> GetMyOrders(CancellationToken ct) =>
        this.ToApiResponse(await orderService.GetMyOrdersAsync(User.GetUserId(), ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMyOrder(int id, CancellationToken ct) =>
        this.ToApiResponse(await orderService.GetMyOrderByIdAsync(User.GetUserId(), id, ct));

    [HttpGet("all")]
    [Authorize(Roles = RoleNames.StaffRoles)]
    public async Task<IActionResult> GetAllOrders(CancellationToken ct) =>
        this.ToApiResponse(await orderService.GetAllOrdersAsync(ct));
}
