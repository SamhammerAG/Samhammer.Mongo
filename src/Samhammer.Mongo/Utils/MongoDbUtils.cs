using System;
using System.Reflection;
using System.Text.RegularExpressions;
using MongoDB.Driver;
using Samhammer.Mongo.Abstractions;

namespace Samhammer.Mongo.Utils
{
    public static class MongoDbUtils
    {
        public static MongoClientSettings GetMongoClientSettings(MongoDbOptions options, string appName = "")
        {
            var settings = MongoClientSettings.FromConnectionString($"mongodb://{options.DatabaseHost}");
            settings.Credential = MongoCredential.CreateCredential(options.AuthDatabaseName ?? options.DatabaseName, options.UserName, options.Password);
            settings.ApplicationName = GetApplicationName(appName);
            return settings;
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
