namespace Application.Api.Features.Auth;

public interface ITokenService
{
    (string token, DateTime expiresAtUtc) CreateAccessToken(User user, List<string> roles);
    string GenerateRefreshToken();
    string HashToken(string token);
}
