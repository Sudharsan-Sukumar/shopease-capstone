using System.Collections.Concurrent;
using Application.Api.Common.Data;
using Application.Api.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Common.Repository;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public IRepository<T> Repository<T>() where T : class =>
        (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new Repository<T>(context));

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);

    public async Task<Result<TResult>> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<Result<TResult>>> operation, CancellationToken ct = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            var result = await operation(ct);

            if (result.IsSuccess)
                await transaction.CommitAsync(ct);
            else
                await transaction.RollbackAsync(ct);

            return result;
        });
    }
}
