namespace DistributedLockTraining.Infrastructure.Abstract
{
    public interface IRedisService
    {
        Task Clear(string key, int databaseIndex = 0);
        List<string> GetKeysByPattern(string pattern, int databaseIndex = 0);
        T GetOrAdd<T>(string key, Func<T> action, int databaseIndex = 0, TimeSpan? expireTime = null) where T : class;
        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> action, int databaseIndex = 0, TimeSpan? expireTime = null) where T : class;
        Task<string> GetValueAsync(string key, int databaseIndex = 0);
        Task<bool> SetValueAsync(string key, string value, int databaseIndex = 0, TimeSpan? expireTime = null);
        Task<bool> AcquireLockAsync(string key, string lockValue, TimeSpan expiration, int databaseIndex = 0);
        Task<bool> ReleaseLockAsync(string key, string lockValue, int databaseIndex = 0);
    }
}
