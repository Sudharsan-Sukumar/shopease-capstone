using Application.Api.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Tests;

/// <summary>
/// Real AppDbContext backed by EF Core's InMemory provider - a fresh,
/// uniquely-named database per call so tests never leak state into each
/// other, but the same Repository/UnitOfWork/Query() LINQ code path the
/// services actually run in production (unlike mocking IQueryable, which
/// breaks on EF's async extension methods like ToListAsync/FirstOrDefaultAsync).
/// Not used for anything that calls IUnitOfWork.ExecuteInTransactionAsync -
/// InMemory has no relational transaction support, so OrderService.CheckoutAsync
/// is covered by live end-to-end testing instead (see project memory).
/// </summary>
internal static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated(); // applies HasData seeds (Roles, Categories) too
        return context;
    }
}
