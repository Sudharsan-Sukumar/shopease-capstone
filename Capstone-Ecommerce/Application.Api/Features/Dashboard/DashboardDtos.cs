namespace Application.Api.Features.Dashboard;

public record DashboardSummaryDto(
    decimal TotalRevenue,
    int TotalOrders,
    int PendingOrders,
    decimal AverageOrderValue,
    int TotalCustomers,
    int TotalProducts,
    int InStockProducts,
    int OutOfStockProducts,
    List<LowStockProductDto> LowStockProducts,
    List<RecentOrderDto> RecentOrders,
    List<MonthlyRevenuePointDto> MonthlyRevenue);

public record LowStockProductDto(int Id, string Name, int Stock);

public record RecentOrderDto(int Id, string CustomerName, int ItemCount, decimal Total, string Status, DateTime CreatedAtUtc);

public record MonthlyRevenuePointDto(string Month, decimal Revenue, int OrderCount);
