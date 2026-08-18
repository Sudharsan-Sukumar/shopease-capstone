using System.ComponentModel.DataAnnotations;

namespace Application.Api.Features.Users;

public record AdminUserDto(int Id, string Email, string FullName, string Phone, List<string> Roles, DateTime CreatedAtUtc);

public record RoleDto(int Id, string Name);

public record CreateUserDto(
    [Required, StringLength(50, MinimumLength = 2)] string FullName,
    [Required, EmailAddress] string Email,
    [Required, RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit mobile number.")] string Phone,
    [Required, MinLength(6), RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
        ErrorMessage = "Password needs an uppercase letter, lowercase letter, digit, and special character.")]
    string Password,
    // Admin picks the role(s) at creation time - unlike self-registration,
    // which is always forced to Customer. This is the "dynamic" part.
    [Required, MinLength(1)] List<string> Roles);

public record UpdateUserRolesDto([Required, MinLength(1)] List<string> Roles);
