using Application.Api.Common.Repository;
using Application.Api.Common.Results;
using Application.Api.Features.Auth;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Features.Users;

public class UserManagementService(IUnitOfWork uow) : IUserManagementService
{
    public async Task<Result<List<AdminUserDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await uow.Repository<User>().Query()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .OrderBy(u => u.Email)
            .ToListAsync(ct);

        return Result.Success(users.Select(ToDto).ToList());
    }

    public async Task<Result<AdminUserDto>> CreateAsync(CreateUserDto dto, IReadOnlyCollection<string> callerRoles, CancellationToken ct = default)
    {
        // Customers are never provisioned from the admin panel - they always
        // self-register via /auth/register. This panel exists for staff accounts only.
        if (dto.Roles.Contains(RoleNames.Customer))
            return Error.Validation("Users.CustomerNotAllowed", "Customer accounts are created via self-registration, not the admin panel.");

        if (!callerRoles.Contains(RoleNames.Admin) && dto.Roles.Any(RoleNames.Privileged.Contains))
            return Error.Forbidden("Users.PrivilegeEscalation", "Sub Admins cannot create Admin or Sub Admin accounts.");

        var exists = await uow.Repository<User>().Query().AnyAsync(u => u.Email == dto.Email, ct);
        if (exists)
            return Error.Conflict("Users.EmailTaken", "An account with this email already exists.");

        var roles = await uow.Repository<Role>().Query().Where(r => dto.Roles.Contains(r.Name)).ToListAsync(ct);
        if (roles.Count != dto.Roles.Distinct().Count())
            return Error.Validation("Users.InvalidRole", "One or more roles do not exist.");

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            FullName = dto.FullName,
            Phone = dto.Phone,
        };
        foreach (var role in roles)
            user.UserRoles.Add(new UserRole { Role = role });

        await uow.Repository<User>().AddAsync(user, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(ToDto(user));
    }

    public async Task<Result<AdminUserDto>> UpdateRolesAsync(int userId, UpdateUserRolesDto dto, IReadOnlyCollection<string> callerRoles, CancellationToken ct = default)
    {
        var user = await uow.Repository<User>().Query()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            return Error.NotFound("Users.NotFound", $"User {userId} was not found.");

        // A Sub Admin can manage other staff, but can neither touch an
        // existing Admin/Sub Admin's roles nor promote anyone INTO those
        // roles - both directions of "no privilege escalation."
        var targetIsPrivileged = user.UserRoles.Any(ur => RoleNames.Privileged.Contains(ur.Role.Name));
        var grantingPrivileged = dto.Roles.Any(RoleNames.Privileged.Contains);
        if (!callerRoles.Contains(RoleNames.Admin) && (targetIsPrivileged || grantingPrivileged))
            return Error.Forbidden("Users.PrivilegeEscalation", "Sub Admins cannot create, edit, or promote Admin or Sub Admin accounts.");

        var roles = await uow.Repository<Role>().Query().Where(r => dto.Roles.Contains(r.Name)).ToListAsync(ct);
        if (roles.Count != dto.Roles.Distinct().Count())
            return Error.Validation("Users.InvalidRole", "One or more roles do not exist.");

        // Mutate the loaded navigation collection directly (not via the
        // generic repository) - that's what lets EF's change tracker pick
        // up both the removals and additions correctly for an explicit
        // join entity like UserRole.
        var toRemove = user.UserRoles.Where(ur => !dto.Roles.Contains(ur.Role.Name)).ToList();
        foreach (var ur in toRemove)
            user.UserRoles.Remove(ur);

        var existingRoleNames = user.UserRoles.Select(ur => ur.Role.Name).ToHashSet();
        foreach (var role in roles.Where(r => !existingRoleNames.Contains(r.Name)))
            user.UserRoles.Add(new UserRole { UserId = user.Id, Role = role });

        // What RoleChangedRule checks on every subsequent request - any
        // token issued before this moment now carries stale role claims.
        user.RolesChangedAtUtc = DateTime.UtcNow;

        await uow.SaveChangesAsync(ct);

        return Result.Success(ToDto(user));
    }

    public async Task<Result> RevokeSessionsAsync(int userId, IReadOnlyCollection<string> callerRoles, CancellationToken ct = default)
    {
        var user = await uow.Repository<User>().Query()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            return Error.NotFound("Users.NotFound", $"User {userId} was not found.");

        // A Supervisor/Support Agent can revoke ordinary sessions for
        // support purposes, but nobody below Admin can kick out an
        // Admin/Sub Admin's session - same escalation guard as role edits.
        if (!callerRoles.Contains(RoleNames.Admin) && user.UserRoles.Any(ur => RoleNames.Privileged.Contains(ur.Role.Name)))
            return Error.Forbidden("Users.PrivilegeEscalation", "You cannot revoke sessions for an Admin or Sub Admin account.");

        // What ManualAdminRevokeRule checks - every token issued before this
        // moment is rejected on its very next request, not just next login.
        user.SessionsRevokedAtUtc = DateTime.UtcNow;
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<List<RoleDto>>> GetRolesAsync(CancellationToken ct = default)
    {
        var roles = await uow.Repository<Role>().Query()
            .Select(r => new RoleDto(r.Id, r.Name))
            .ToListAsync(ct);

        return Result.Success(roles);
    }

    private static AdminUserDto ToDto(User u) => new(
        u.Id, u.Email, u.FullName, u.Phone,
        u.UserRoles.Select(ur => ur.Role.Name).ToList(),
        u.CreatedAtUtc);
}
