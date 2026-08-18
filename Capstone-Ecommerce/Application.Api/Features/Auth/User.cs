namespace Application.Api.Features.Auth;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Each independently bumped by a different trigger, each checked by its
    // own IRevocationRule against an access token's "iat" claim - a token
    // issued BEFORE the relevant timestamp is treated as revoked, even
    // though it hasn't naturally expired yet. Kept as three separate
    // columns (not one shared "security stamp") so each rule demonstrates
    // its own distinct real-world trigger.
    public DateTime PasswordChangedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? RolesChangedAtUtc { get; set; }
    public DateTime? SessionsRevokedAtUtc { get; set; }

    public List<UserRole> UserRoles { get; set; } = [];
    public List<RefreshToken> RefreshTokens { get; set; } = [];
}
