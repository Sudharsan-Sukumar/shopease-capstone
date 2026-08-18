using System.ComponentModel.DataAnnotations;

namespace Application.Api.Features.Auth;

public record RegisterDto(
    [Required, StringLength(50, MinimumLength = 2)] string FullName,
    [Required, EmailAddress] string Email,
    [Required, RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit mobile number.")] string Phone,
    [Required, MinLength(6), RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
        ErrorMessage = "Password needs an uppercase letter, lowercase letter, digit, and special character.")]
    string Password);

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record RefreshDto([Required] string RefreshToken);

public record ChangePasswordDto(
    [Required] string CurrentPassword,
    [Required, MinLength(6), RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
        ErrorMessage = "Password needs an uppercase letter, lowercase letter, digit, and special character.")]
    string NewPassword);

public record UserDto(int Id, string Email, string FullName, string Phone, List<string> Roles);

public record AuthResultDto(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAtUtc, UserDto User);
