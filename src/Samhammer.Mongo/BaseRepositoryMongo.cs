using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Samhammer.Mongo.Abstractions;
using Samhammer.Mongo.Utils;

namespace Samhammer.Mongo
{
    public class BaseRepositoryMongo<T> : IBaseRepositoryMongo<T>
        where T : BaseModelMongo
    {
        public ILogger<BaseRepositoryMongo<T>> Logger { get; }

        public IMongoCollection<T> Collection { get; }

        private IMongoDatabase Database { get; }

        public BaseRepositoryMongo(ILogger<BaseRepositoryMongo<T>> logger, IMongoDbConnector connector)
        {
            Logger = logger;
            Database = connector.GetMongoDatabase();
            Collection = GetCollection<T>();
        }

        protected IMongoCollection<TR> GetCollection<TR>()
        {
            var collectionName = MongoDbUtils.GetCollectionName<TR>();
            return Database.GetCollection<TR>(collectionName);
        }

        public virtual async Task<T> GetById(string id)
        {
            Logger.LogTrace("Loading model with objectId {ObjectId} from {Collection} collection", id, Collection.CollectionNamespace);
            var entries = await Collection.FindAsync(d => d.Id.Equals(id));
            return entries.FirstOrDefault();
        }

        public virtual Task<List<T>> GetAll()
        {
            Logger.LogTrace("Loading all models from {Collection} collection", Collection.CollectionNamespace);
            return Collection.AsQueryable().ToListAsync();
        }

        public virtual async Task Save(T model)
        {
            if (model.IsPersistent())
            {
                var result = await Collection.ReplaceOneAsync(d => d.Id == model.Id, model);

                if (result.MatchedCount == 1)
                {
                    Logger.LogTrace("Updated one model of type {ModelType} with ObjectId {ObjectId} in {Collection} collection", typeof(T), model.Id, Collection.CollectionNamespace);
                }
                else if (result.MatchedCount > 1)
                {
                    Logger.LogWarning("Updated multiple models of type {ModelType} with ObjectId {ObjectId} in {Collection} collection", typeof(T), model.Id, Collection.CollectionNamespace);
                }
                else
                {
                    Logger.LogWarning("Updated zero models of type {ModelType} with ObjectId {ObjectId} in {Collection} collection", typeof(T), model.Id, Collection.CollectionNamespace);
                    throw new MongoRepositoryException("update failed");
                }
            }
            else
            {
                await Collection.InsertOneAsync(model);
                Logger.LogTrace("Inserted model of type {ModelType} with ObjectId {ObjectId} into {Collection} collection", typeof(T), model.Id, Collection.CollectionNamespace);
            }
        }

        public virtual async Task Delete(T model)
        {
            if (model.IsPersistent())
            {
                await Collection.DeleteOneAsync(d => d.Id == model.Id);
                Logger.LogTrace("Deleted model of type {ModelType} with ObjectId {ObjectId} from {Collection} collection", typeof(T), model.Id, Collection.CollectionNamespace);
            }
        }

        public virtual async Task DeleteAll()
        {
            await Collection.DeleteManyAsync(d => true);
            Logger.LogTrace("Deleted all models from {Collection} collection", Collection.CollectionNamespace);
        }
    }
}
