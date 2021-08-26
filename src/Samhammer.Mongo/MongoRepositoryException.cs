using System;

namespace Samhammer.Mongo
{
    public class MongoRepositoryException : Exception
    {
        public MongoRepositoryException(string message)
            : base(message)
        {
        }
    }
}
