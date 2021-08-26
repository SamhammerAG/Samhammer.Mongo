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
    }
}
