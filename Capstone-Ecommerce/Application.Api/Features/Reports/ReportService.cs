using System.Globalization;
using System.Text;
using Application.Api.Common.Repository;
using Application.Api.Common.Results;
using Application.Api.Features.Orders;
using Application.Api.Features.Products;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Features.Reports;

public class ReportService(IUnitOfWork uow) : IReportService
{
    public async Task<Result<SalesSummaryDto>> GetSalesReportAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var query = uow.Repository<Order>().Query()
            .Include(o => o.User)
            .Include(o => o.Items)
            .Where(o => o.Status == OrderStatus.Confirmed);

        if (from is not null) query = query.Where(o => o.CreatedAtUtc >= from);
        if (to is not null) query = query.Where(o => o.CreatedAtUtc < to.Value.Date.AddDays(1));

        var orders = await query.OrderByDescending(o => o.CreatedAtUtc).ToListAsync(ct);

        var rows = orders
            .Select(o => new SalesReportRowDto(o.Id, o.User.Email, o.CreatedAtUtc, o.Status.ToString(), o.Items.Count, o.Total))
            .ToList();

        var totalRevenue = orders.Sum(o => o.Total);
        var averageOrderValue = orders.Count > 0 ? totalRevenue / orders.Count : 0m;

        return Result.Success(new SalesSummaryDto(totalRevenue, orders.Count, averageOrderValue, rows));
    }

    public async Task<Result<InventoryReportDto>> GetInventoryReportAsync(CancellationToken ct = default)
    {
        var products = await uow.Repository<Product>().Query()
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

        var rows = products
            .Select(p => new InventoryReportRowDto(p.Id, p.Name, p.Category.Name, p.Price, p.Stock, p.Price * p.Stock))
            .ToList();

        return Result.Success(new InventoryReportDto(rows.Count, rows.Sum(r => r.StockValue), rows));
    }

    public async Task<Result<string>> ExportSalesCsvAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var report = await GetSalesReportAsync(from, to, ct);
        if (!report.IsSuccess) return report.Error;

        var csv = new StringBuilder();
        csv.AppendLine("OrderId,CustomerEmail,CreatedAtUtc,Status,ItemCount,Total");
        foreach (var row in report.Value.Orders)
        {
            csv.AppendLine(string.Join(',',
                row.OrderId,
                CsvField(row.CustomerEmail),
                row.CreatedAtUtc.ToString("O", CultureInfo.InvariantCulture),
                row.Status,
                row.ItemCount,
                row.Total.ToString(CultureInfo.InvariantCulture)));
        }

        return Result.Success(csv.ToString());
    }

    private static string CsvField(string value) =>
        value.Contains(',') || value.Contains('"')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}
