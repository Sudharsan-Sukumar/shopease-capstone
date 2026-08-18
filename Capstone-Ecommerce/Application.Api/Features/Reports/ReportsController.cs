using System.Text;
using Application.Api.Common.Results;
using Application.Api.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Features.Reports;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = RoleNames.StaffRoles)]
public class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("sales")]
    public async Task<IActionResult> GetSalesReport([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct) =>
        this.ToApiResponse(await reportService.GetSalesReportAsync(from, to, ct));

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventoryReport(CancellationToken ct) =>
        this.ToApiResponse(await reportService.GetInventoryReportAsync(ct));

    // Deliberately NOT wrapped in ApiResponse<T> - a file download is a raw
    // byte stream with its own Content-Type/Content-Disposition, the exact
    // point of this endpoint versus every other JSON one in the app.
    [HttpGet("sales/export")]
    public async Task<IActionResult> ExportSalesCsv([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var result = await reportService.ExportSalesCsvAsync(from, to, ct);
        if (!result.IsSuccess) return this.ToApiResponse(result);

        var bytes = Encoding.UTF8.GetBytes(result.Value);
        var fileName = $"sales-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }
}
