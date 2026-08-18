using Application.Api.Features.Auth;
using Application.Api.Features.Auth.Revocation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Api.Tests;

[TestFixture]
public class RevocationRuleEngineTests
{
    private static User NewUser() => new() { Email = "u@test.com", FullName = "U", PasswordHash = "x" };

    [Test]
    public void IsRevoked_ReturnsFalse_WhenNoRuleMatches()
    {
        // Arrange
        var user = NewUser();
        var engine = new RevocationRuleEngine([new PasswordChangedRule(), new RoleChangedRule()], NullLogger<RevocationRuleEngine>.Instance);

        // Act
        var revoked = engine.IsRevoked(user, DateTime.UtcNow);

        // Assert
        Assert.That(revoked, Is.False);
    }

    [Test]
    public void IsRevoked_ReturnsTrue_WhenAnyRuleMatches()
    {
        // Arrange
        var user = NewUser();
        user.PasswordChangedAtUtc = DateTime.UtcNow;
        var tokenIssuedAt = DateTime.UtcNow.AddMinutes(-5);
        var engine = new RevocationRuleEngine([new PasswordChangedRule()], NullLogger<RevocationRuleEngine>.Instance);

        // Act
        var revoked = engine.IsRevoked(user, tokenIssuedAt);

        // Assert
        Assert.That(revoked, Is.True);
    }

    [Test]
    public void IsRevoked_ShortCircuits_OnFirstMatchingRule()
    {
        // Arrange - a rule placed AFTER a matching one must never be invoked,
        // proven with a rule that throws if it's ever called.
        var user = NewUser();
        user.PasswordChangedAtUtc = DateTime.UtcNow;
        var tokenIssuedAt = DateTime.UtcNow.AddMinutes(-5);
        var engine = new RevocationRuleEngine([new PasswordChangedRule(), new ThrowingRule()], NullLogger<RevocationRuleEngine>.Instance);

        // Act & Assert
        Assert.That(() => engine.IsRevoked(user, tokenIssuedAt), Throws.Nothing);
    }

    private class ThrowingRule : IRevocationRule
    {
        public string Name => "Throwing";
        public bool IsRevoked(User user, DateTime tokenIssuedAtUtc) => throw new InvalidOperationException("Should not be called - engine failed to short-circuit.");
    }
}
