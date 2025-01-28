using MongoDB.Driver;

namespace RealStateApi.Infrastructure.Data
{
    public interface IMongoDbContext
    {
        IMongoCollection<T> GetCollection<T>(string collectionName);
    }
}
