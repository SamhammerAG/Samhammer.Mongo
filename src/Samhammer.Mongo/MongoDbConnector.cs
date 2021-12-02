using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Events.Diagnostics;
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

        public async Task<bool> Ping()
        {
            var db = GetMongoDatabase();
            var ping = await db
                .RunCommandAsync<BsonDocument>(new BsonDocument { { "ping", 1 } }, default);

            if (ping.TryGetValue("ok", out var ok))
            {
                return ok.Equals(1.0) || ok.Equals(1);
            }

            return false;
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

                if (Options.Value.TraceDriver)
                {
                    cb.Subscribe(GetCSharpDriverLogger());
                }
            };

            return new MongoClient(mongoClientSettings);
        }

        private TraceSourceEventSubscriber GetCSharpDriverLogger()
        {
            var logFilename = "mongo.log";
            File.Delete(logFilename);

            var fileStream = new FileStream(logFilename, FileMode.Append);
            var listener = new TextWriterTraceListener(fileStream) { TraceOutputOptions = TraceOptions.DateTime };

            var traceSource = new TraceSource("CSHARPDRIVER", SourceLevels.All);
            traceSource.Listeners.Clear();
            traceSource.Listeners.Add(listener);
            
            return new TraceSourceEventSubscriber(traceSource);
        }
    }

    public interface IMongoDbConnector
    {
        IMongoDatabase GetMongoDatabase();

        MongoClient GetOrCreateConnection();

        Task<bool> Ping();
    }
}
