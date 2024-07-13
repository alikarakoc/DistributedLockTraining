namespace DistributedLockTraining.Infrastructure.Abstract
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(bool includeDeleted = false);
        Task<T> GetAsync(int id);
        Task<T> GetByColumnAsync(string columnName, object value);
        Task<T> UpdateAsync(T entity);
        Task<T> AddAsync(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entity);
        Task<bool> DeleteAsync(T entity);
        Task<T> QuerySingleAsync(string query, object parameters);
        Task<IEnumerable<T>> QueryListAsync(string query, object parameters);
        Task<TResult> QueryAsync<TResult>(string query, object parameters);
    }
}
