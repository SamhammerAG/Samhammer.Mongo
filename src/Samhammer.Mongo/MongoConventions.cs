using MongoDB.Bson.Serialization.Conventions;

namespace Samhammer.Mongo
{
    public class MongoConventions : IMongoConventions
    {
        public void Register()
        {
            var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
            ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);
        }
    }

    public interface IMongoConventions
    {
        void Register();
    }
}
