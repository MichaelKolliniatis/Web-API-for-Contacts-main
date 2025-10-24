using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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

        public async Task<List<TResult>> GetAllProjectedAsync<TResult>(AutoMapper.IConfigurationProvider mapperConfig, bool asNoTracking = true, CancellationToken ct = default)
        {
            IQueryable<T> q = _set;
            if (asNoTracking) q = q.AsNoTracking();

            return await q.ProjectTo<TResult>(mapperConfig).ToListAsync(ct);
        }

        public async Task<T?> GetByIdAsync(int id, bool asNoTracking = true, CancellationToken ct = default)
        {
            var entity = await _set.FindAsync([id], ct);
            if (entity is null) return null;
            if (asNoTracking) _context.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        public async Task<TResult?> GetByIdProjectedAsync<TResult>(int id, AutoMapper.IConfigurationProvider mapperConfig, CancellationToken ct = default)
        {
            var dto = await _set
                .AsNoTracking()
                .Where(e => EF.Property<int>(e, "Id") == id)
                .ProjectTo<TResult>(mapperConfig)
                .SingleOrDefaultAsync(ct);

            return dto;
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
            if (entity is not null)
                await DeleteAsync(entity, ct);
        }

        public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            return _set.AnyAsync(predicate, ct);
        }

        public Task<List<TOut>> SelectWhereAsync<TOut>(Expression<Func<T, bool>> predicate, Expression<Func<T, TOut>> selector, bool asNoTracking = true, CancellationToken ct = default)
        {
            IQueryable<T> q = _set;
            if (asNoTracking) q = q.AsNoTracking();

            return q.Where(predicate).Select(selector).ToListAsync(ct);
        }
        public Task<int> DeleteWhereAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            return _set.Where(predicate).ExecuteDeleteAsync(ct);
        }
    }
}
