namespace Application.Api.Features.Auth.Revocation;

/// <summary>A token issued before an admin last changed this user's roles still carries the OLD role claims - reject it so the user must re-authenticate and get a token reflecting the new roles.</summary>
public class RoleChangedRule : IRevocationRule
{
    public string Name => "RoleChanged";

    public bool IsRevoked(User user, DateTime tokenIssuedAtUtc) =>
        user.RolesChangedAtUtc is { } changedAt && changedAt.TruncateToSeconds() > tokenIssuedAtUtc;
}
