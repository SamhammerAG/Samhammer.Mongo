using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Samhammer.Mongo.Abstractions;

namespace Samhammer.Mongo.Utils
{
    public static class MongoDbUtils
    {
        public static MongoClientSettings GetMongoClientSettings(MongoDbOptions options, DatabaseCredential credential, string appName = "")
        {
            var settings = MongoClientSettings.FromConnectionString(options.ConnectionString);

            if (!string.IsNullOrEmpty(credential.UserName))
            {
                settings.Credential = MongoCredential.CreateCredential(
                    credential.AuthDatabaseName ?? credential.DatabaseName,
                    credential.UserName,
                    credential.Password);
            }

            settings.ApplicationName = GetApplicationName(appName);
            return settings;
        }

        public static string GetMongoUrl(IConfiguration configuration, DatabaseCredential credential = null)
        {
            if (credential == null)
            {
                var credentialsSection = configuration.GetSection($"{nameof(MongoDbOptions)}:{nameof(MongoDbOptions.DatabaseCredentials)}");
                var credentials = credentialsSection.Get<List<DatabaseCredential>>();

                credential = credentials.First();
            }

            var db = GetTruncateMongoDb(credential.DatabaseName);
            var authDb = GetTruncateMongoDb(credential.AuthDatabaseName) ?? db;
            var connectionString = GetMongoConnectionString(configuration);

            var mongoUrlBuilder = new MongoUrlBuilder(connectionString)
            {
                AuthenticationSource = authDb,
                Username = credential.UserName,
                Password = credential.Password,
            };

            return mongoUrlBuilder.ToString();
        }

        public static string GetTruncateMongoDb(string databaseName)
        {
            return databaseName?
                .Truncate(MongoDbOptions.MaxDatabaseNameLength)
                .ToLower();
        }
        
        private static string GetMongoConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration[$"{nameof(MongoDbOptions)}:{nameof(MongoDbOptions.ConnectionString)}"];
            var dbHost = configuration[$"{nameof(MongoDbOptions)}:{nameof(MongoDbOptions.DatabaseHost)}"];
            return !string.IsNullOrEmpty(connectionString) ? connectionString : $"mongodb://{dbHost}";
        }

        private static string GetApplicationName(string identifier = "")
        {
            var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
            return StringJoinUtils.JoinIgnoreEmpty(" ", assemblyName, identifier);
        }

        public static string GetCollectionName<T>()
        {
            return GetCollectionName(typeof(T));
        }

        public static string GetCollectionName(Type type)
        {
            var attribute = type.GetTypeInfo().GetCustomAttribute<MongoCollectionAttribute>(true);

            if (!string.IsNullOrEmpty(attribute?.CollectionName))
            {
                return attribute.CollectionName;
            }

            return type.Name.RemoveString("Model").ToLowerFirstChar();
        }

        public static bool IsValidObjectId(string id)
        {
            var validObjectIdRegex = new Regex(@"^[0-9a-fA-F]{24}$");
            return id != null && validObjectIdRegex.IsMatch(id);
        }
    }
}
