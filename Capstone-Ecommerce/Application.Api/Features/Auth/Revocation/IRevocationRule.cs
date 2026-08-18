namespace Application.Api.Features.Auth.Revocation;

/// <summary>
/// One pluggable check: "given this user and when their access token was
/// issued, should that token be treated as revoked?" Each implementation
/// covers exactly one real-world trigger. Adding a new rule later means
/// adding a new class and registering it in Program.cs - never touching
/// an existing rule or RevocationRuleEngine itself (Open/Closed).
/// </summary>
public interface IRevocationRule
{
    /// <summary>Short, stable name used in logs so it's obvious which rule fired.</summary>
    string Name { get; }

    bool IsRevoked(User user, DateTime tokenIssuedAtUtc);
}
