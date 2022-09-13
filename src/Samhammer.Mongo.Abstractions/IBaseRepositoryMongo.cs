using System.Collections.Generic;
using System.Threading.Tasks;

namespace Samhammer.Mongo.Abstractions
{
    public interface IBaseRepositoryMongo<T> where T : BaseModelMongo
    {
        Task<T> GetById(string id);

        Task<List<T>> GetAll();

        Task Save(T model);

        Task Create(T model);

        Task Delete(T model);

        Task DeleteById(string id);

        Task DeleteAll();
    }
}
