using System;
using System.Collections.Concurrent;
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

        private static readonly ConcurrentDictionary<string, MongoClient> Clients = new ConcurrentDictionary<string, MongoClient>();

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
            var mongoClient = GetMongoClient();
            return mongoClient.GetDatabase(Options.Value.DatabaseName);
        }

        public MongoClient GetMongoClient()
        {
            var connectionKey = $"{Options.Value.DatabaseHost}|{Options.Value.DatabaseName}|{Options.Value.UserName}";
            return Clients.GetOrAdd(connectionKey, _ => CreateMongoClient());
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
    }
}
