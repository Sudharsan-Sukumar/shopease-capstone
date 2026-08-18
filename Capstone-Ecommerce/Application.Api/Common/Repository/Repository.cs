using Application.Api.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Common.Repository;

public class Repository<T>(AppDbContext context) : IRepository<T> where T : class
{
    private readonly DbSet<T> _set = context.Set<T>();

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) => await _set.FindAsync([id], ct);
    public async Task<List<T>> GetAllAsync(CancellationToken ct = default) => await _set.ToListAsync(ct);
    public IQueryable<T> Query() => _set.AsQueryable();
    public async Task AddAsync(T entity, CancellationToken ct = default) => await _set.AddAsync(entity, ct);
    public void Update(T entity) => _set.Update(entity);
    public void Remove(T entity) => _set.Remove(entity);
}
