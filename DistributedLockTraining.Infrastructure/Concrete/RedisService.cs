using DistributedLockTraining.Infrastructure.Abstract;
using StackExchange.Redis;
using System.Text.Json;

namespace DistributedLockTraining.Infrastructure.Concrete
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _redisCon;

        public RedisService(IConnectionMultiplexer redisCon)
        {
            _redisCon = redisCon;
        }

        public async Task Clear(string key, int databaseIndex = 0)
        {
            var db = _redisCon.GetDatabase(databaseIndex);
            await db.KeyDeleteAsync(key);
        }

        public List<string> GetKeysByPattern(string pattern, int databaseIndex = 0)
        {
            var server = _redisCon.GetServer(_redisCon.GetEndPoints().First());
            var db = _redisCon.GetDatabase(databaseIndex);
            var keys = new List<string>();
            var redisKeys = server.Keys(pattern: pattern, database: databaseIndex);
            foreach (var redisKey in redisKeys)
                keys.Add(redisKey.ToString());
            return keys;
        }

        public T GetOrAdd<T>(string key, Func<T> action, int databaseIndex = 0, TimeSpan? expireTime = null) where T : class
        {
            var db = _redisCon.GetDatabase(databaseIndex);
            var result = db.StringGet(key);
            if (result.IsNull)
            {
                var value = action();
                var serializedValue = JsonSerializer.SerializeToUtf8Bytes(value);
                db.StringSet(key, serializedValue, expireTime);
                return value;
            }
            return JsonSerializer.Deserialize<T>(result);
        }

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> action, int databaseIndex = 0, TimeSpan? expireTime = null) where T : class
        {
            var db = _redisCon.GetDatabase(databaseIndex);
            var result = await db.StringGetAsync(key);
            if (result.IsNull)
            {
                var value = await action();
                var serializedValue = JsonSerializer.SerializeToUtf8Bytes(value);
                await db.StringSetAsync(key, serializedValue, expireTime);
                return value;
            }
            return JsonSerializer.Deserialize<T>(result);
        }

        public async Task<string> GetValueAsync(string key, int databaseIndex = 0)
        {
            var db = _redisCon.GetDatabase(databaseIndex);
            return await db.StringGetAsync(key);
        }

        public async Task<bool> SetValueAsync(string key, string value, int databaseIndex = 0, TimeSpan? expireTime = null)
        {
            var db = _redisCon.GetDatabase(databaseIndex);
            return await db.StringSetAsync(key, value, expireTime);
        }
        public async Task<bool> AcquireLockAsync(string key, string lockValue, TimeSpan expiration, int databaseIndex = 0)
        {
            var db = _redisCon.GetDatabase(databaseIndex);
            return await db.LockTakeAsync(key, lockValue, expiration);
        }

        public async Task<bool> ReleaseLockAsync(string key, string lockValue, int databaseIndex = 0)
        {
            var db = _redisCon.GetDatabase(databaseIndex);
            return await db.LockReleaseAsync(key, lockValue);
        }
    }

}
