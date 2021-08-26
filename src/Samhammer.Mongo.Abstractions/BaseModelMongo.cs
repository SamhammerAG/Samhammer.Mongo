using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Samhammer.Mongo.Abstractions
{
    public class BaseModelMongo
    {
        [BsonId]
        [BsonIgnoreIfDefault]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public bool IsPersistent()
        {
            return !string.IsNullOrEmpty(Id);
        }
    }
}
