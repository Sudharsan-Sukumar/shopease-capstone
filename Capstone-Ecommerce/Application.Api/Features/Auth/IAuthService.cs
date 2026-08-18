using Application.Api.Common.Results;

namespace Application.Api.Features.Auth;

public interface IAuthService
{
    Task<Result<AuthResultDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    Task<Result<AuthResultDto>> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<Result<AuthResultDto>> RefreshAsync(string refreshToken, CancellationToken ct = default);
    Task<Result> LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task<Result> ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken ct = default);
}
