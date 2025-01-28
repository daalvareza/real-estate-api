using MongoDB.Driver;
using System.Threading.Tasks;
using RealEstateApi.Domain.Entities;
using RealEstateApi.Infrastructure.Data;
using RealEstateApi.Application.Interfaces;

namespace RealEstateApi.Infrastructure.Repositories
{
    public class OwnerRepository : IOwnerRepository
    {
        private readonly IMongoCollection<Owner> _ownerCollection;

        public OwnerRepository(IMongoDbContext mongoDbContext)
        {
            _ownerCollection = mongoDbContext.GetCollection<Owner>("Owners");
        }

        public async Task<Owner?> GetOwnerByIdAsync(string idOwner)
        {
            var filter = Builders<Owner>.Filter.Eq(x => x.Id, idOwner);
            return await _ownerCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Owner?> GetOwnerByEmailAsync(string email)
        {
            var filter = Builders<Owner>.Filter.Eq(x => x.Email, email);
            return await _ownerCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateOwnerAsync(Owner owner)
        {
            await _ownerCollection.InsertOneAsync(owner);
        }
    }
}
