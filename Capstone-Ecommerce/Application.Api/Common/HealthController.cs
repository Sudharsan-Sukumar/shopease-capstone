using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Common;

/// <summary>
/// Not a business feature - infrastructure-level, so it lives in Common/
/// rather than Features/. Phase 0's sole purpose: prove Angular can reach
/// the API at all (CORS, ports, routing) before any real feature exists.
/// </summary>
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { status = "ok", timestampUtc = DateTime.UtcNow });
}
