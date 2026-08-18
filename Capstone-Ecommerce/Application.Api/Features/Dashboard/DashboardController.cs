using Application.Api.Common.Results;
using Application.Api.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Features.Dashboard;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = RoleNames.StaffRoles)]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct) =>
        this.ToApiResponse(await dashboardService.GetSummaryAsync(from, to, ct));
}
