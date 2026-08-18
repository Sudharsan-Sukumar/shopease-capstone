using Application.Api.Common.Results;
using Application.Api.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Features.Products;

[ApiController]
[Route("api/products")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] string? search, CancellationToken ct) =>
        this.ToApiResponse(await productService.GetAllAsync(search, ct));

    // Staff-only, includes inactive products - what lets an admin find and
    // reactivate a previously "deleted" (soft-deleted) product. Route
    // constraint on {id:int} below means "all" never gets swallowed by it.
    [HttpGet("all")]
    [Authorize(Roles = RoleNames.ProductManagers)]
    public async Task<IActionResult> GetAllForAdmin(CancellationToken ct) =>
        this.ToApiResponse(await productService.GetAllForAdminAsync(ct));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken ct) =>
        this.ToApiResponse(await productService.GetByIdAsync(id, ct));

    [HttpGet("/api/categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories(CancellationToken ct) =>
        this.ToApiResponse(await productService.GetCategoriesAsync(ct));

    [HttpPost]
    [Authorize(Roles = RoleNames.ProductManagers)]
    public async Task<IActionResult> Create(CreateProductDto dto, CancellationToken ct) =>
        this.ToApiResponse(await productService.CreateAsync(dto, ct));

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.ProductManagers)]
    public async Task<IActionResult> Update(int id, UpdateProductDto dto, CancellationToken ct) =>
        this.ToApiResponse(await productService.UpdateAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.ProductManagers)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        this.ToApiResponse(await productService.DeleteAsync(id, ct));
}
