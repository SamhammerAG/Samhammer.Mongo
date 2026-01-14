using System;
using System.Collections.Generic;
using System.Linq;
using HealthChecks.MongoDb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Samhammer.Mongo.Utils;

namespace Samhammer.Mongo.Health
{
    public static class HealthChecksBuilderExtensions
    {
        public static IHealthChecksBuilder AddMongoDb(
            this IHealthChecksBuilder builder,
            string name = null,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null,
            TimeSpan? timeout = null)
        {
            return builder.Add(new HealthCheckRegistration(name ?? "mongodb", serviceProvider => GetMongoDbHealthCheck(serviceProvider), failureStatus, tags, timeout));
        }

        public static IHealthChecksBuilder AddMongoDb(
            this IHealthChecksBuilder builder,
            List<DatabaseCredential> credentials,
            string name = null,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null,
            TimeSpan? timeout = null)
        {
            if (credentials == null || !credentials.Any())
            {
                throw new ArgumentException("At least one database credential must be provided.");
            }

            foreach (var credential in credentials)
            {
                var healthCheckName = $"{name ?? "mongodb"}_{credential.DatabaseName}";
                builder.Add(new HealthCheckRegistration(
                    healthCheckName,
                    serviceProvider => GetMongoDbHealthCheck(serviceProvider, credential),
                    failureStatus,
                    tags,
                    timeout));
            }

            return builder;
        }

        private static MongoDbHealthCheck GetMongoDbHealthCheck(IServiceProvider serviceProvider, DatabaseCredential credential = null)
        {
            if (credential == null)
            {
                var mongoDbOptions = serviceProvider.GetRequiredService<IOptions<MongoDbOptions>>();
                credential = mongoDbOptions.Value.DatabaseCredentials[0];
            }

            var mongoClientSettings = MongoDbUtils.GetMongoClientSettings(credential, "health");
            var client = new MongoClient(mongoClientSettings);
            return new MongoDbHealthCheck(client, credential.DatabaseName);
        }
    }
}
