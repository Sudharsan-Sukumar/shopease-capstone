using Application.Api.Common.Results;

namespace Application.Api.Features.Dashboard;

public interface IDashboardService
{
    Task<Result<DashboardSummaryDto>> GetSummaryAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
}
