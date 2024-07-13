using DistributedLockTraining.Infrastructure.Abstract;
using DistributedLockTraining.Infrastructure.Concrete;
using DistributedLockTraining.Infrastructure.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace DistributedLockTraining.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServiceRegistration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DapperDbContext>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IRedisService, RedisService>();
            var redisConn = ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis"));
            services.AddSingleton<IConnectionMultiplexer>(redisConn);
            var multiplexer = new List<RedLockMultiplexer> {
                ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis"))
            };
            var redlockFactory = RedLockFactory.Create(multiplexer);
            services.AddSingleton<IDistributedLockFactory>(redlockFactory);
            return services;
        }
    }
}
