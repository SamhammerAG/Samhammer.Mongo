using System.Collections.Generic;

namespace Samhammer.Mongo
{
    public class MongoDbOptions
    {
        public const int MaxDatabaseNameLength = 63;

        public List<DatabaseCredential> DatabaseCredentials { get; set; }

        public string DatabaseHost { get; set; }

        private string connectionString;

        public string ConnectionString
        {
            get => !string.IsNullOrEmpty(connectionString) ? connectionString : $"mongodb://{DatabaseHost}";
            set => connectionString = value;
        }

        public bool TraceDriver { get; set; }
    }
}
