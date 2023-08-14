using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        private static readonly Dictionary<string, MongoClient> Clients = new Dictionary<string, MongoClient>();
        private static readonly object InitializeLock = new object();

        public MongoDbConnector(IOptions<MongoDbOptions> options, ILogger<MongoDbConnector> logger, IMongoConventions conventions)
        {
            Options = options;
            Logger = logger;
            Conventions = conventions;
        }

        public IMongoDatabase GetMongoDatabase(string databaseName)
        {
            var credential = GetCredential(databaseName);
            var mongoClient = GetMongoClient(credential);
            return mongoClient.GetDatabase(credential.DatabaseName);
        }
        
        public MongoClient GetMongoClient(DatabaseCredential credential)
        {
            lock (InitializeLock)
            {
                if (!Clients.TryGetValue(credential.DatabaseName, out var client))
                {
                    client = InitClient(credential);
                    Clients[credential.DatabaseName] = client;
                }

                return client;
            }
        }
        
        public async Task<bool> Ping(string databaseName)
        {
            var db = GetMongoDatabase(databaseName);
            var ping = await db
                .RunCommandAsync<BsonDocument>(new BsonDocument { { "ping", 1 } }, default);

            if (ping.TryGetValue("ok", out var ok))
            {
                return ok.Equals(1.0) || ok.Equals(1);
            }

            return false;
        }

        private MongoClient InitClient(DatabaseCredential credential)
        {
            Logger.LogInformation(
                "Creating new connection to database {DatabaseName} on {ConnectionString} as user {UserName}",
                credential.DatabaseName,
                credential.ConnectionString,
                credential.UserName);

            Conventions.Register();

            var mongoClientSettings = MongoDbUtils.GetMongoClientSettings(credential);
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

        private DatabaseCredential GetCredential(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                databaseName = Options.Value.DatabaseCredentials[0].DatabaseName;
            }

            var credential = Options.Value.DatabaseCredentials.FirstOrDefault(c => c.DatabaseName.Equals(databaseName, StringComparison.OrdinalIgnoreCase));
            return credential ?? throw new ArgumentException($"Database credentials not found for database: {databaseName}");
        }
    }

    public interface IMongoDbConnector
    {
        IMongoDatabase GetMongoDatabase(string databaseName);

        MongoClient GetMongoClient(DatabaseCredential credential);

        Task<bool> Ping(string databaseName);
    }
}
