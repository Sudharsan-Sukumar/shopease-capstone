namespace Application.Api.Features.Auth;

/// <summary>Join entity, composite key (UserId, RoleId) - configured in UserRoleConfiguration.</summary>
public class UserRole
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
}
