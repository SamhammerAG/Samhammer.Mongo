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

        private IMongoConventions Conventions { get; }

        private static MongoClient client;

        private static bool initialized ;

        private static object initializeLock = new object();

        public MongoDbConnector(IOptions<MongoDbOptions> options, ILogger<MongoDbConnector> logger, IMongoConventions conventions)
        {
            Options = options;
            Logger = logger;
            Conventions = conventions;
        }

        public IMongoDatabase GetMongoDatabase()
        {
            var mongoClient = GetMongoClient();
            return mongoClient.GetDatabase(Options.Value.DatabaseName);
        }

        public MongoClient GetMongoClient()
        {
            return LazyInitializer.EnsureInitialized(ref client, ref initialized, ref initializeLock, InitClient);
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

        private MongoClient InitClient()
        {
            Logger.LogInformation(
                "Creating new connection to database {DatabaseName} on {ConnectionString} as user {UserName}",
                Options.Value.DatabaseName,
                Options.Value.ConnectionString,
                Options.Value.UserName);

            Conventions.Register();

            var mongoClientSettings = MongoDbUtils.GetMongoClientSettings(Options.Value);
            RegisterLogging(mongoClientSettings);

            return new MongoClient(mongoClientSettings);
        }

        private void RegisterLogging(MongoClientSettings mongoClientSettings)
        {
            mongoClientSettings.ClusterConfigurator = cb =>
            {
                cb.Subscribe<CommandStartedEvent>(e => Logger.LogTrace("mongodb command: {Command}", e.Command.ToJson()));

                if (Options.Value.TraceDriver)
                {
                    cb.Subscribe(GetCSharpDriverLogger());
                }
            };
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

        MongoClient GetMongoClient();

        Task<bool> Ping();
    }
}
