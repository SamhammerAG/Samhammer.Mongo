using Samhammer.Mongo.Abstractions;

namespace Samhammer.Mongo.Test
{
    [MongoCollection("testUser")]
    public class TestUserModel : BaseModelMongo
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
