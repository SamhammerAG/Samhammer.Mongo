using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Samhammer.Mongo.Utils;

namespace Samhammer.Mongo
{
    public static class MongoDbExtensions
    {
        public static IServiceCollection AddMongoDb(this IServiceCollection services, Action<MongoDbOptions> configure)
        {
            services.Configure(configure);
            services.AddMongoDbServices();
            return services;
        }

        public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDbOptions>(configuration.GetSection(nameof(MongoDbOptions)));
            services.AddMongoDbServices();
            return services;
        }

        private static void AddMongoDbServices(this IServiceCollection services)
        {
            services.AddSingleton<IMongoDbConnector, MongoDbConnector>();
            services.AddSingleton<IMongoConventions, MongoConventions>();
            services.AddInitConnectionService();
            services.PostConfigure<MongoDbOptions>(PostConfigureMongo);
        }

        private static void AddInitConnectionService(this IServiceCollection services)
        {
            services.AddHostedService<InitializeConnectionService>();
        }

        private static void PostConfigureMongo(MongoDbOptions options)
        {
            options.DatabaseName = options.DatabaseName.Truncate(MongoDbOptions.MaxDatabaseNameLength).ToLower();
        }
    }
}
