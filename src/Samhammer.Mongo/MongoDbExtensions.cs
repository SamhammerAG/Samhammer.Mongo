using System;
using System.Dynamic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Samhammer.Mongo.Utils;

namespace Samhammer.Mongo
{
    public static class MongoDbExtensions
    {
        public static void AddMongoDb(this IServiceCollection services, Action<MongoDbOptions> configure)
        {
            services.Configure(configure);
            services.AddMongoDb();
        }

        public static void AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDbOptions>(configuration.GetSection(nameof(MongoDbOptions)));
            services.AddMongoDb();
        }

        private static void AddMongoDb(this IServiceCollection services)
        {
            services.AddSingleton<IMongoDbConnector, MongoDbConnector>();
            services.AddSingleton<IMongoConventions, MongoConventions>();
            services.PostConfigure<MongoDbOptions>(PostConfigureMongo);
        }

        private static void PostConfigureMongo(MongoDbOptions options)
        {
            options.DatabaseName = options.DatabaseName.Truncate(MongoDbOptions.MaxDatabaseNameLength).ToLower();
        }

        public static void CreateMongoDbConnection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHostedService<InitializeConnectionService>();
        }
    }
}
