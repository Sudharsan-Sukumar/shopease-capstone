using Application.Api.Common.Results;

namespace Application.Api.Tests;

/// <summary>Unit tests for the Result/Result&lt;T&gt; pattern every service in the app relies on for expected-failure handling.</summary>
[TestFixture]
public class ResultTests
{
    [Test]
    public void Success_ReturnsIsSuccessTrue_WithNoError()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Error, Is.EqualTo(Error.None));
    }

    [Test]
    public void Failure_ReturnsIsSuccessFalse_WithGivenError()
    {
        // Arrange
        var error = Error.NotFound("Test.NotFound", "not found");

        // Act
        var result = Result.Failure(error);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(error));
    }

    [Test]
    public void Value_ThrowsInvalidOperationException_WhenResultIsFailure()
    {
        // Arrange
        Result<int> result = Error.Validation("Test.Invalid", "bad");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Test]
    public void ImplicitConversion_FromValue_ProducesSuccessResult()
    {
        // Act
        Result<string> result = "hello";

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("hello"));
    }

    [Test]
    public void ImplicitConversion_FromError_ProducesFailureResult()
    {
        // Arrange
        var error = Error.Conflict("Test.Conflict", "dup");

        // Act
        Result<string> result = error;

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(error));
    }

    [Test]
    public void Failure_Throws_WhenGivenErrorNone()
    {
        // A failed result must carry a real error - Error.None means "no failure happened".
        Assert.Throws<InvalidOperationException>(() => Result.Failure<int>(Error.None));
    }
}
