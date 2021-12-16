namespace Samhammer.Mongo
{
    public class MongoDbOptions
    {
        public const int MaxDatabaseNameLength = 63;

        public string UserName { get; set; }

        public string Password { get; set; }

        public string AuthDatabaseName { get; set; }

        public string DatabaseName { get; set; }

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
