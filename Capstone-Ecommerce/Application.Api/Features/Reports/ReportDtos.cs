namespace Application.Api.Features.Reports;

public record SalesReportRowDto(int OrderId, string CustomerEmail, DateTime CreatedAtUtc, string Status, int ItemCount, decimal Total);

public record SalesSummaryDto(decimal TotalRevenue, int OrderCount, decimal AverageOrderValue, List<SalesReportRowDto> Orders);

public record InventoryReportRowDto(int ProductId, string Name, string CategoryName, decimal Price, int Stock, decimal StockValue);

public record InventoryReportDto(int TotalProducts, decimal TotalStockValue, List<InventoryReportRowDto> Products);
