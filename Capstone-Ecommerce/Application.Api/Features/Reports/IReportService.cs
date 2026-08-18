using Application.Api.Common.Results;

namespace Application.Api.Features.Reports;

public interface IReportService
{
    Task<Result<SalesSummaryDto>> GetSalesReportAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<Result<InventoryReportDto>> GetInventoryReportAsync(CancellationToken ct = default);

    /// <summary>Same underlying data as GetSalesReportAsync, rendered as CSV text - the controller wraps this as a file download, not a JSON envelope.</summary>
    Task<Result<string>> ExportSalesCsvAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
}
