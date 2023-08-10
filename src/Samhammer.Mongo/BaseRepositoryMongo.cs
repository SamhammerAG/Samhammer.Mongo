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
        protected ILogger<BaseRepositoryMongo<T>> Logger { get; }

        protected IMongoCollection<T> Collection { get; }

        private IMongoDatabase Database { get; }

        protected FilterDefinitionBuilder<T> Filter => Builders<T>.Filter;

        protected UpdateDefinitionBuilder<T> Update => Builders<T>.Update;
        
        public BaseRepositoryMongo(ILogger<BaseRepositoryMongo<T>> logger, IMongoDbConnector connector, string databaseName = null)
        {
            Logger = logger;
            Database = connector.GetMongoDatabase(databaseName);
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
                var result = await Collection.ReplaceOneAsync(d => d.Id == model.Id, model, new ReplaceOptions { IsUpsert = true });

                if (result.MatchedCount == 0)
                {
                    Logger.LogTrace("Inserted model of type {ModelType} with ObjectId {ObjectId} into {Collection} collection", typeof(T), model.Id, Collection.CollectionNamespace);
                }
                else
                {
                    Logger.LogTrace("Updated model of type {ModelType} with ObjectId {ObjectId} in {Collection} collection", typeof(T), model.Id, Collection.CollectionNamespace);
                }
            }
            else
            {
                await Collection.InsertOneAsync(model);
                Logger.LogTrace("Inserted model of type {ModelType} with ObjectId {ObjectId} into {Collection} collection", typeof(T), model.Id, Collection.CollectionNamespace);
            }
        }

        public async Task Create(T model)
        {
            await Collection.InsertOneAsync(model);
            Logger.LogTrace("Inserted model of type {ModelType} with ObjectId {ObjectId} into {Collection} collection", typeof(T), model.Id, Collection.CollectionNamespace);
        }

        public virtual async Task Delete(T model)
        {
            if (model.IsPersistent())
            {
                await Collection.DeleteOneAsync(d => d.Id == model.Id);
                Logger.LogTrace("Deleted model of type {ModelType} with ObjectId {ObjectId} from {Collection} collection", typeof(T), model.Id, Collection.CollectionNamespace);
            }
        }

        public async Task DeleteById(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                await Collection.DeleteOneAsync(d => d.Id == id);
                Logger.LogTrace("Deleted model of type {ModelType} with ObjectId {ObjectId} from {Collection} collection", typeof(T), id, Collection.CollectionNamespace);
            }
        }

        public virtual async Task DeleteAll()
        {
            await Collection.DeleteManyAsync(d => true);
            Logger.LogTrace("Deleted all models from {Collection} collection", Collection.CollectionNamespace);
        }
    }
}
