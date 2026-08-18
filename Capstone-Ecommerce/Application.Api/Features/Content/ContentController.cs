using Application.Api.Common.Results;
using Application.Api.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Features.Content;

[ApiController]
[Route("api/content")]
public class ContentController(IContentService contentService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive(CancellationToken ct) =>
        this.ToApiResponse(await contentService.GetActiveAsync(ct));

    [HttpGet("all")]
    [Authorize(Roles = RoleNames.StaffRoles)]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        this.ToApiResponse(await contentService.GetAllAsync(ct));

    [HttpPost]
    [Authorize(Roles = RoleNames.ContentManagers)]
    public async Task<IActionResult> Create(CreateContentBlockDto dto, CancellationToken ct) =>
        this.ToApiResponse(await contentService.CreateAsync(dto, ct));

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.ContentManagers)]
    public async Task<IActionResult> Update(int id, UpdateContentBlockDto dto, CancellationToken ct) =>
        this.ToApiResponse(await contentService.UpdateAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.ContentManagers)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        this.ToApiResponse(await contentService.DeleteAsync(id, ct));
}
