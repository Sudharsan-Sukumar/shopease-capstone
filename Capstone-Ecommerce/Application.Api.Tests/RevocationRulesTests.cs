using Application.Api.Features.Auth;
using Application.Api.Features.Auth.Revocation;

namespace Application.Api.Tests;

/// <summary>
/// Covers the three IRevocationRule implementations, including a regression
/// test for the same-second truncation bug found during Phase 3 live
/// verification: JWT `iat` is whole-second precision but the User timestamp
/// columns carry milliseconds, so a naive `>` comparison falsely revoked a
/// token issued in the same wall-clock second as the triggering change.
/// </summary>
[TestFixture]
public class RevocationRulesTests
{
    private static User NewUser() => new() { Email = "u@test.com", FullName = "U", PasswordHash = "x" };

    [Test]
    public void PasswordChangedRule_IsRevoked_WhenChangeIsClearlyAfterToken()
    {
        // Arrange
        var user = NewUser();
        var tokenIssuedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        user.PasswordChangedAtUtc = tokenIssuedAt.AddSeconds(2);
        var rule = new PasswordChangedRule();

        // Act
        var revoked = rule.IsRevoked(user, tokenIssuedAt);

        // Assert
        Assert.That(revoked, Is.True);
    }

    [Test]
    public void PasswordChangedRule_IsNotRevoked_WhenChangeIsInTheSameSecondAsToken()
    {
        // Arrange - change happens 300ms into the SAME second the token was issued.
        var user = NewUser();
        var tokenIssuedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        user.PasswordChangedAtUtc = tokenIssuedAt.AddMilliseconds(300);
        var rule = new PasswordChangedRule();

        // Act
        var revoked = rule.IsRevoked(user, tokenIssuedAt);

        // Assert
        Assert.That(revoked, Is.False);
    }

    [Test]
    public void PasswordChangedRule_IsNotRevoked_WhenChangeWasBeforeTheToken()
    {
        // Arrange
        var user = NewUser();
        var tokenIssuedAt = new DateTime(2026, 1, 1, 12, 0, 5, DateTimeKind.Utc);
        user.PasswordChangedAtUtc = tokenIssuedAt.AddSeconds(-10);
        var rule = new PasswordChangedRule();

        // Act
        var revoked = rule.IsRevoked(user, tokenIssuedAt);

        // Assert
        Assert.That(revoked, Is.False);
    }

    [Test]
    public void RoleChangedRule_IsNotRevoked_WhenRolesNeverChanged()
    {
        // Arrange - RolesChangedAtUtc defaults to null (never set).
        var user = NewUser();
        var rule = new RoleChangedRule();

        // Act
        var revoked = rule.IsRevoked(user, DateTime.UtcNow);

        // Assert
        Assert.That(revoked, Is.False);
    }

    [Test]
    public void RoleChangedRule_IsRevoked_WhenRolesChangedAfterToken()
    {
        // Arrange
        var user = NewUser();
        var tokenIssuedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        user.RolesChangedAtUtc = tokenIssuedAt.AddSeconds(5);
        var rule = new RoleChangedRule();

        // Act
        var revoked = rule.IsRevoked(user, tokenIssuedAt);

        // Assert
        Assert.That(revoked, Is.True);
    }

    [Test]
    public void RoleChangedRule_IsNotRevoked_WhenChangeIsInTheSameSecondAsToken()
    {
        // Arrange - same truncation edge case as PasswordChangedRule.
        var user = NewUser();
        var tokenIssuedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        user.RolesChangedAtUtc = tokenIssuedAt.AddMilliseconds(700);
        var rule = new RoleChangedRule();

        // Act
        var revoked = rule.IsRevoked(user, tokenIssuedAt);

        // Assert
        Assert.That(revoked, Is.False);
    }

    [Test]
    public void ManualAdminRevokeRule_IsNotRevoked_WhenSessionsNeverManuallyRevoked()
    {
        // Arrange
        var user = NewUser();
        var rule = new ManualAdminRevokeRule();

        // Act
        var revoked = rule.IsRevoked(user, DateTime.UtcNow);

        // Assert
        Assert.That(revoked, Is.False);
    }

    [Test]
    public void ManualAdminRevokeRule_IsRevoked_WhenRevokedAfterToken()
    {
        // Arrange
        var user = NewUser();
        var tokenIssuedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        user.SessionsRevokedAtUtc = tokenIssuedAt.AddSeconds(1);
        var rule = new ManualAdminRevokeRule();

        // Act
        var revoked = rule.IsRevoked(user, tokenIssuedAt);

        // Assert
        Assert.That(revoked, Is.True);
    }
}
