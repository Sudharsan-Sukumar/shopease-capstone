namespace Application.Api.Features.Auth.Revocation;

/// <summary>An admin's explicit kill-switch (POST /api/users/{id}/revoke-sessions) - every token issued before that moment is instantly rejected, regardless of why the admin pulled it.</summary>
public class ManualAdminRevokeRule : IRevocationRule
{
    public string Name => "ManualAdminRevoke";

    public bool IsRevoked(User user, DateTime tokenIssuedAtUtc) =>
        user.SessionsRevokedAtUtc is { } revokedAt && revokedAt.TruncateToSeconds() > tokenIssuedAtUtc;
}
