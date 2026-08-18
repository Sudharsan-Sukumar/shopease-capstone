using Microsoft.Extensions.Logging;

namespace Application.Api.Features.Auth.Revocation;

/// <summary>
/// Runs every registered IRevocationRule against a user/token pair, short-
/// circuiting on the first match. Resolving IEnumerable&lt;IRevocationRule&gt;
/// via DI is what makes this Open/Closed in practice - Program.cs registers
/// each rule once, and this class never needs to know the concrete list.
/// </summary>
public class RevocationRuleEngine(IEnumerable<IRevocationRule> rules, ILogger<RevocationRuleEngine> logger)
{
    public bool IsRevoked(User user, DateTime tokenIssuedAtUtc)
    {
        foreach (var rule in rules)
        {
            if (rule.IsRevoked(user, tokenIssuedAtUtc))
            {
                logger.LogInformation(
                    "Session rejected for user {UserId} by rule {Rule} (token issued {IssuedAt:o})",
                    user.Id, rule.Name, tokenIssuedAtUtc);
                return true;
            }
        }

        return false;
    }
}
