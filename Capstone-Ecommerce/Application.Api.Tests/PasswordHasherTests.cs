using Application.Api.Features.Auth;

namespace Application.Api.Tests;

[TestFixture]
public class PasswordHasherTests
{
    [Test]
    public void Verify_ReturnsTrue_ForCorrectPassword()
    {
        // Arrange
        var hash = PasswordHasher.Hash("Correct@123");

        // Act
        var result = PasswordHasher.Verify("Correct@123", hash);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void Verify_ReturnsFalse_ForIncorrectPassword()
    {
        // Arrange
        var hash = PasswordHasher.Hash("Correct@123");

        // Act
        var result = PasswordHasher.Verify("Wrong@123", hash);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void Hash_ProducesDifferentHashes_ForTheSamePasswordCalledTwice()
    {
        // Salted per call - the reason the Admin user must be seeded at
        // runtime rather than via a fixed EF HasData hash (see DbSeeder).

        // Act
        var hash1 = PasswordHasher.Hash("Sample@123");
        var hash2 = PasswordHasher.Hash("Sample@123");

        // Assert
        Assert.That(hash1, Is.Not.EqualTo(hash2));
        Assert.That(PasswordHasher.Verify("Sample@123", hash1), Is.True);
        Assert.That(PasswordHasher.Verify("Sample@123", hash2), Is.True);
    }

    [Test]
    public void Verify_ReturnsFalse_ForMalformedStoredHash()
    {
        // Act
        var result = PasswordHasher.Verify("anything", "not-a-valid-hash-format");

        // Assert
        Assert.That(result, Is.False);
    }
}
