using System.Security.Claims;
using Application.Api.Common.Results;
using Application.Api.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Application.Api.Features.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterDto dto, CancellationToken ct) =>
        this.ToApiResponse(await authService.RegisterAsync(dto, ct));

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginDto dto, CancellationToken ct) =>
        this.ToApiResponse(await authService.LoginAsync(dto, ct));

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Refresh(RefreshDto dto, CancellationToken ct) =>
        this.ToApiResponse(await authService.RefreshAsync(dto.RefreshToken, ct));

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(RefreshDto dto, CancellationToken ct) =>
        this.ToApiResponse(await authService.LogoutAsync(dto.RefreshToken, ct));

    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto, CancellationToken ct) =>
        this.ToApiResponse(await authService.ChangePasswordAsync(User.GetUserId(), dto, ct));

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        return Ok(ApiResponse<object>.Ok(new { email, roles }));
    }
}
