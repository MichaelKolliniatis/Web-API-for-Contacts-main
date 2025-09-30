using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Web_API_for_Contacts_2._0.Data.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ContactsDbContext _context;
        private readonly DbSet<T> _set;

        public GenericRepository(ContactsDbContext context)
        {
            _context = context;
            _set = context.Set<T>();
        }

        public async Task<List<T>> GetAllAsync(bool asNoTracking = true, CancellationToken ct = default)
        {
            var q = asNoTracking ? _set.AsNoTracking() : _set;
            return await q.ToListAsync(ct);
        }

        public async Task<T?> GetByIdAsync(int id, bool asNoTracking = true, CancellationToken ct = default)
        {
            // Fast path (single int key)
            var entity = await _set.FindAsync([id], ct);
            if (entity is null) return null;
            if (asNoTracking) _context.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        public async Task AddAsync(T entity, CancellationToken ct = default)
        {
            await _set.AddAsync(entity, ct);
        }

        public Task UpdateAsync(T entity, CancellationToken ct = default)
        {
            _set.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity, CancellationToken ct = default)
        {
            _set.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task DeleteByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await GetByIdAsync(id, asNoTracking: false, ct);
            if (entity is not null) _set.Remove(entity);
        }

        public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            return _set.AnyAsync(predicate, ct);
        }
    }
}
