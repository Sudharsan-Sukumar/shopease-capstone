using Application.Api.Common.Repository;
using Application.Api.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Features.Auth;

public class AuthService(IUnitOfWork uow, ITokenService tokenService, IConfiguration config) : IAuthService
{
    public async Task<Result<AuthResultDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        var exists = await uow.Repository<User>().Query().AnyAsync(u => u.Email == dto.Email, ct);
        if (exists)
            return Error.Conflict("Auth.EmailTaken", "An account with this email already exists.");

        var customerRole = await uow.Repository<Role>().Query().FirstOrDefaultAsync(r => r.Name == "Customer", ct);
        if (customerRole is null)
            return Error.Unexpected("Auth.RolesNotSeeded", "Customer role is missing - roles must be seeded.");

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            FullName = dto.FullName,
            Phone = dto.Phone,
        };
        user.UserRoles.Add(new UserRole { Role = customerRole });

        await uow.Repository<User>().AddAsync(user, ct);
        await uow.SaveChangesAsync(ct);

        var (resultDto, _) = await IssueTokensAsync(user, ["Customer"], ct);
        return Result.Success(resultDto);
    }

    public async Task<Result<AuthResultDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await uow.Repository<User>().Query()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == dto.Email, ct);

        if (user is null || !PasswordHasher.Verify(dto.Password, user.PasswordHash))
            return Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var (resultDto, _) = await IssueTokensAsync(user, roles, ct);
        return Result.Success(resultDto);
    }

    public async Task<Result<AuthResultDto>> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var tokenHash = tokenService.HashToken(refreshToken);
        var stored = await uow.Repository<RefreshToken>().Query()
            .Include(rt => rt.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);

        if (stored is null)
            return Error.Unauthorized("Auth.InvalidRefreshToken", "Refresh token is invalid.");

        if (!stored.IsActive)
            return Error.Unauthorized("Auth.RefreshTokenExpired", "Refresh token is no longer active.");

        stored.RevokedAtUtc = DateTime.UtcNow;

        var roles = stored.User.UserRoles.Select(ur => ur.Role.Name).ToList();
        var (resultDto, newTokenEntity) = await IssueTokensAsync(stored.User, roles, ct);
        stored.ReplacedByTokenId = newTokenEntity.Id;
        await uow.SaveChangesAsync(ct);

        return Result.Success(resultDto);
    }

    public async Task<Result> LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var tokenHash = tokenService.HashToken(refreshToken);
        var stored = await uow.Repository<RefreshToken>().Query().FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);

        if (stored is null)
            return Result.Success(); // already gone - logout is idempotent

        stored.RevokedAtUtc = DateTime.UtcNow;
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken ct = default)
    {
        var user = await uow.Repository<User>().GetByIdAsync(userId, ct);
        if (user is null)
            return Error.NotFound("Auth.UserNotFound", "User not found.");

        if (!PasswordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
            return Error.Validation("Auth.InvalidCurrentPassword", "Current password is incorrect.");

        user.PasswordHash = PasswordHasher.Hash(dto.NewPassword);
        // Bumping this is what PasswordChangedRule checks on every
        // subsequent request - every access token issued before this
        // moment (including the one used to make THIS request) stops
        // working the instant this SaveChangesAsync commits.
        user.PasswordChangedAtUtc = DateTime.UtcNow;
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    private async Task<(AuthResultDto dto, RefreshToken tokenEntity)> IssueTokensAsync(User user, List<string> roles, CancellationToken ct)
    {
        var (accessToken, expiresAtUtc) = tokenService.CreateAccessToken(user, roles);
        var refreshTokenRaw = tokenService.GenerateRefreshToken();

        var refreshTokenDays = double.Parse(config["Jwt:RefreshTokenDays"]!);
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenService.HashToken(refreshTokenRaw),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(refreshTokenDays),
        };

        await uow.Repository<RefreshToken>().AddAsync(refreshToken, ct);
        await uow.SaveChangesAsync(ct);

        await EnforceMaxConcurrentSessionsAsync(user.Id, ct);

        var userDto = new UserDto(user.Id, user.Email, user.FullName, user.Phone, roles);
        var dto = new AuthResultDto(accessToken, refreshTokenRaw, expiresAtUtc, userDto);
        return (dto, refreshToken);
    }

    /// <summary>
    /// Issuance-time cap, deliberately NOT part of RevocationRuleEngine -
    /// that engine's three rules are all checked per-request against a
    /// single User-level timestamp, which works for "invalidate
    /// everything since moment X" but can't express "keep the N newest
    /// sessions, drop only the rest" (a per-user timestamp can't single out
    /// specific sessions). So this revokes the oldest REFRESH tokens
    /// directly instead. Effect is intentionally softer than the other
    /// three rules: the bumped session's still-valid ACCESS token keeps
    /// working until it naturally expires (up to Jwt:AccessTokenMinutes) -
    /// only its next refresh attempt will fail, since that's the layer
    /// this actually revokes at.
    /// </summary>
    private async Task EnforceMaxConcurrentSessionsAsync(int userId, CancellationToken ct)
    {
        var maxSessions = int.Parse(config["Jwt:MaxConcurrentSessions"] ?? "3");

        var activeSessions = await uow.Repository<RefreshToken>().Query()
            .Where(rt => rt.UserId == userId && rt.RevokedAtUtc == null && rt.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(rt => rt.CreatedAtUtc)
            .ToListAsync(ct);

        if (activeSessions.Count <= maxSessions) return;

        var now = DateTime.UtcNow;
        foreach (var session in activeSessions.Skip(maxSessions)) // keep the N newest, revoke the rest
            session.RevokedAtUtc = now;

        await uow.SaveChangesAsync(ct);
    }
}
