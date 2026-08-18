using Application.Api.Common.Results;

namespace Application.Api.Common.Repository;

public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Explicit transaction for multi-table writes (e.g. checkout).
    /// Runs through the DB's execution strategy (SQL Server's
    /// EnableRetryOnFailure) rather than a raw BeginTransactionAsync,
    /// because EF Core requires that: a retry strategy needs to be able to
    /// redo the WHOLE transaction atomically on a transient failure, which
    /// it can't safely do if the transaction boundaries are managed outside
    /// its control. Commits on a successful Result, rolls back otherwise -
    /// callers return an Error Result for expected failures (e.g.
    /// insufficient stock, concurrency conflict) instead of throwing.
    /// </summary>
    Task<Result<TResult>> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<Result<TResult>>> operation, CancellationToken ct = default);
}
