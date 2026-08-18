using Application.Api.Common.Results;

namespace Application.Api.Features.Users;

public interface IUserManagementService
{
    Task<Result<List<AdminUserDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<AdminUserDto>> CreateAsync(CreateUserDto dto, IReadOnlyCollection<string> callerRoles, CancellationToken ct = default);
    Task<Result<AdminUserDto>> UpdateRolesAsync(int userId, UpdateUserRolesDto dto, IReadOnlyCollection<string> callerRoles, CancellationToken ct = default);
    Task<Result> RevokeSessionsAsync(int userId, IReadOnlyCollection<string> callerRoles, CancellationToken ct = default);
    Task<Result<List<RoleDto>>> GetRolesAsync(CancellationToken ct = default);
}
