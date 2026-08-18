using Application.Api.Common.Repository;
using Application.Api.Common.Results;
using Application.Api.Features.Auth;
using Application.Api.Features.Orders;
using Application.Api.Features.Products;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Features.Dashboard;

public class DashboardService(IUnitOfWork uow) : IDashboardService
{
    public async Task<Result<DashboardSummaryDto>> GetSummaryAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var ordersQuery = uow.Repository<Order>().Query()
            .Include(o => o.User)
            .Include(o => o.Items);

        IQueryable<Order> filtered = ordersQuery;
        if (from is not null) filtered = filtered.Where(o => o.CreatedAtUtc >= from);
        // Inclusive of the whole "to" day, not just midnight.
        if (to is not null) filtered = filtered.Where(o => o.CreatedAtUtc < to.Value.Date.AddDays(1));

        var orders = await filtered.ToListAsync(ct);
        var completedOrders = orders.Where(o => o.Status == OrderStatus.Confirmed).ToList();

        var totalRevenue = completedOrders.Sum(o => o.Total);
        var averageOrderValue = completedOrders.Count > 0 ? totalRevenue / completedOrders.Count : 0m;

        var totalCustomers = await uow.Repository<User>().Query()
            .CountAsync(u => u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Customer), ct);

        var products = await uow.Repository<Product>().Query().Where(p => p.IsActive).ToListAsync(ct);
        var inStock = products.Count(p => p.Stock > 0);

        var lowStock = products
            .Where(p => p.Stock <= 10)
            .OrderBy(p => p.Stock)
            .Take(10)
            .Select(p => new LowStockProductDto(p.Id, p.Name, p.Stock))
            .ToList();

        var recentOrders = orders
            .OrderByDescending(o => o.CreatedAtUtc)
            .Take(8)
            .Select(o => new RecentOrderDto(o.Id, o.User.FullName, o.Items.Count, o.Total, o.Status.ToString(), o.CreatedAtUtc))
            .ToList();

        var monthlyRevenue = completedOrders
            .GroupBy(o => new { o.CreatedAtUtc.Year, o.CreatedAtUtc.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyRevenuePointDto(
                new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                g.Sum(o => o.Total),
                g.Count()))
            .ToList();

        return Result.Success(new DashboardSummaryDto(
            totalRevenue,
            orders.Count,
            orders.Count(o => o.Status == OrderStatus.PaymentPending),
            averageOrderValue,
            totalCustomers,
            products.Count,
            inStock,
            products.Count - inStock,
            lowStock,
            recentOrders,
            monthlyRevenue));
    }
}
