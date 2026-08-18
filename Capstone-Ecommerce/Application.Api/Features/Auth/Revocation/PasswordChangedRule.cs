namespace Application.Api.Features.Auth.Revocation;

/// <summary>A token issued before the user's last password change is stale - the old password may have been compromised, which is exactly why they changed it.</summary>
public class PasswordChangedRule : IRevocationRule
{
    public string Name => "PasswordChanged";

    public bool IsRevoked(User user, DateTime tokenIssuedAtUtc) =>
        user.PasswordChangedAtUtc.TruncateToSeconds() > tokenIssuedAtUtc;
}
