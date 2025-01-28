using MongoDB.Driver;

namespace RealEstateApi.Infrastructure.Data
{
    public interface IMongoDbContext
    {
        IMongoCollection<T> GetCollection<T>(string collectionName);
    }
}
