using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using Samhammer.Mongo.Utils;

namespace Samhammer.Mongo
{
    public class MongoDbConnector : IMongoDbConnector
    {
        private IOptions<MongoDbOptions> Options { get; }

        private ILogger<MongoDbConnector> Logger { get; }

        private readonly Lazy<Action> initConventions;

        private static MongoClient client;

        public MongoDbConnector(IOptions<MongoDbOptions> options, ILogger<MongoDbConnector> logger, IMongoConventions conventions)
        {
            Options = options;
            Logger = logger;

            initConventions = new Lazy<Action>(() =>
            {
                conventions.Register();
                return () => { };
            });
        }

        public IMongoDatabase GetMongoDatabase()
        {
            GetOrCreateConnection();
            return client.GetDatabase(Options.Value.DatabaseName);
        }

        public MongoClient GetOrCreateConnection()
        {
            return LazyInitializer.EnsureInitialized(ref client, CreateMongoClient);
        }

        private MongoClient CreateMongoClient()
        {
            Logger.LogInformation(
                "Creating new connection to database {DatabaseName} on {DatabaseHost} as user {UserName}",
                Options.Value.DatabaseName,
                Options.Value.DatabaseHost,
                Options.Value.UserName);

            initConventions.Value();

            var mongoClientSettings = MongoDbUtils.GetMongoClientSettings(Options.Value);
            mongoClientSettings.ClusterConfigurator = cb =>
            {
                cb.Subscribe<CommandStartedEvent>(e => Logger.LogTrace("MongoDb command: {Command}", e.Command.ToJson()));
            };

            return new MongoClient(mongoClientSettings);
        }
    }

    public interface IMongoDbConnector
    {
        IMongoDatabase GetMongoDatabase();

        MongoClient GetOrCreateConnection();
    }
}
