using System;
using System.Collections.Generic;
using HealthChecks.MongoDb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
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
            return builder.Add(new HealthCheckRegistration(name ?? "mongodb", GetMongoDbHealthCheck, failureStatus, tags, timeout));
        }

        private static MongoDbHealthCheck GetMongoDbHealthCheck(IServiceProvider serviceProvider)
        {
            var mongoDbOptions = serviceProvider.GetRequiredService<IOptions<MongoDbOptions>>();
            var mongoClientSettings = MongoDbUtils.GetMongoClientSettings(mongoDbOptions.Value, "health");
            return new MongoDbHealthCheck(mongoClientSettings, mongoDbOptions.Value.DatabaseName);
        }
    }
}
