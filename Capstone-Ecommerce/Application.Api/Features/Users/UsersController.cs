using Application.Api.Common.Results;
using Application.Api.Common.Security;
using Application.Api.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Features.Users;

[ApiController]
[Route("api/users")]
[Authorize(Roles = RoleNames.StaffRoles)]
public class UsersController(IUserManagementService userManagementService) : ControllerBase
{
    // Every staff tier can see the list - Supervisor/Support Agent need it
    // to find who to revoke, even though they can't create or edit roles.
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        this.ToApiResponse(await userManagementService.GetAllAsync(ct));

    [HttpPost]
    [Authorize(Roles = RoleNames.UserManagers)]
    public async Task<IActionResult> Create(CreateUserDto dto, CancellationToken ct) =>
        this.ToApiResponse(await userManagementService.CreateAsync(dto, User.GetRoles(), ct));

    [HttpPut("{id:int}/roles")]
    [Authorize(Roles = RoleNames.UserManagers)]
    public async Task<IActionResult> UpdateRoles(int id, UpdateUserRolesDto dto, CancellationToken ct) =>
        this.ToApiResponse(await userManagementService.UpdateRolesAsync(id, dto, User.GetRoles(), ct));

    [HttpPost("{id:int}/revoke-sessions")]
    public async Task<IActionResult> RevokeSessions(int id, CancellationToken ct) =>
        this.ToApiResponse(await userManagementService.RevokeSessionsAsync(id, User.GetRoles(), ct));

    [HttpGet("/api/roles")]
    [Authorize(Roles = RoleNames.UserManagers)]
    public async Task<IActionResult> GetRoles(CancellationToken ct) =>
        this.ToApiResponse(await userManagementService.GetRolesAsync(ct));
}
