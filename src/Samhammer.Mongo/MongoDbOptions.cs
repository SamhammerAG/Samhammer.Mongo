using System.Collections.Generic;

namespace Samhammer.Mongo
{
    public class MongoDbOptions
    {
        public const int MaxDatabaseNameLength = 63;

        public List<DatabaseCredential> DatabaseCredentials { get; set; }

        public bool TraceDriver { get; set; }
    }
}
