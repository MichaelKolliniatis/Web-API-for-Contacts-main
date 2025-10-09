using System.Linq.Expressions;

namespace Web_API_for_Contacts_2._0.Data.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        //Task<List<T>> GetAllAsync(bool asNoTracking = true, CancellationToken ct = default);
        Task<List<TResult>> GetAllProjectedAsync<TResult>(AutoMapper.IConfigurationProvider mapperConfig, bool asNoTracking = true, CancellationToken ct = default);
        Task<T?> GetByIdAsync(int id, bool asNoTracking = true, CancellationToken ct = default);
        Task<TResult?> GetByIdProjectedAsync<TResult>(int id, AutoMapper.IConfigurationProvider mapperConfig, CancellationToken ct = default);
        Task AddAsync(T entity, CancellationToken ct = default);
        Task UpdateAsync(T entity, CancellationToken ct = default);
        Task DeleteAsync(T entity, CancellationToken ct = default);
        Task DeleteByIdAsync(int id, CancellationToken ct = default);

        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    }
}
