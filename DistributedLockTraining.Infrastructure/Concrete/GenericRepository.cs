using Dapper;
using Dapper.Contrib.Extensions;
using DistributedLockTraining.Infrastructure.Abstract;
using DistributedLockTraining.Infrastructure.Context;
using System.Reflection;

namespace DistributedLockTraining.Infrastructure.Concrete
{
    public class GenericRepository<T>(DapperDbContext context) : IGenericRepository<T> where T : class
    {
        private readonly DapperDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<IEnumerable<T>> GetAllAsync(bool includeDeleted = false)
        {
            await using var connection = _context.GetConnection();
            var tableName = GetTableName(typeof(T));
            var query = includeDeleted
                ? $"SELECT * FROM {tableName} WITH(NOLOCK)"
                : $"SELECT * FROM {tableName} WITH(NOLOCK) WHERE Deleted = 0";
            return await connection.QueryAsync<T>(query);
        }

        public async Task<T> GetAsync(int id)
        {
            await using var connection = _context.GetConnection();
            return await connection.GetAsync<T>(id);
        }

        public async Task<T> GetByColumnAsync(string columnName, object value)
        {
            var validColumns = typeof(T).GetProperties().Select(p => p.Name).ToList();
            if (!validColumns.Contains(columnName))
            {
                throw new ArgumentException("Invalid column name", nameof(columnName));
            }

            var tableName = GetTableName(typeof(T));
            var query = $"SELECT * FROM {tableName} WITH(NOLOCK) WHERE {columnName} = @Value";
            await using var connection = _context.GetConnection();
            return await connection.QueryFirstOrDefaultAsync<T>(query, new { Value = value });
        }

        public async Task<T> UpdateAsync(T entity)
        {
            await using var connection = _context.GetConnection();
            await connection.UpdateAsync(entity);
            return entity;
        }

        public async Task<T> AddAsync(T entity)
        {
            await using var connection = _context.GetConnection();
            await connection.InsertAsync(entity);
            return entity;
        }

        public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entity)
        {
            await using var connection = _context.GetConnection();
            await connection.InsertAsync(entity);
            return entity;
        }

        public async Task<bool> DeleteAsync(T entity)
        {
            using var connection = _context.GetConnection();
            return await connection.DeleteAsync(entity);
        }

        public async Task<T> QuerySingleAsync(string query, object parameters)
        {
            if (string.IsNullOrEmpty(query))
                throw new ArgumentException("Query cannot be null or empty", nameof(query));

            await using var connection = _context.GetConnection();
            var data = await connection.QueryAsync<T>(query, parameters);
            return data.FirstOrDefault();
        }

        public async Task<IEnumerable<T>> QueryListAsync(string query, object parameters)
        {
            if (string.IsNullOrEmpty(query))
                throw new ArgumentException("Query cannot be null or empty", nameof(query));

            await using var connection = _context.GetConnection();
            return await connection.QueryAsync<T>(query, parameters);
        }

        public async Task<TResult> QueryAsync<TResult>(string query, object parameters)
        {
            if (string.IsNullOrEmpty(query))
                throw new ArgumentException("Query cannot be null or empty", nameof(query));

            await using var connection = _context.GetConnection();
            var data = await connection.QueryAsync<T>(query, parameters);

            if (typeof(TResult) == typeof(T))
                return (TResult)(object)data.FirstOrDefault();
            else if (typeof(TResult) == typeof(IEnumerable<T>))
                return (TResult)(object)data.ToList();
            else
                throw new InvalidOperationException("Invalid return type");
        }


        private string GetTableName(Type type)
        {
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            if (tableAttribute != null)
            {
                return tableAttribute.Name;
            }

            throw new InvalidOperationException("Table attribute not found on type " + type.Name);
        }
    }

}
