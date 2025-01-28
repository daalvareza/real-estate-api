using MongoDB.Driver;
using System.Threading.Tasks;
using RealEstateApi.Domain.Entities;
using RealEstateApi.Infrastructure.Data;
using RealEstateApi.Infrastructure.DataModels;
using RealEstateApi.Application.Interfaces;

namespace RealEstateApi.Infrastructure.Repositories
{
    public class PropertyImageRepository : IPropertyImageRepository
    {
        private readonly IMongoCollection<PropertyImageDataModel> _propertyImageCollection;

        public PropertyImageRepository(IMongoDbContext context)
        {
            _propertyImageCollection = context.GetCollection<PropertyImageDataModel>("PropertyImage");
        }

        public async Task<PropertyImage?> GetFirstImageAsync(string idProperty)
        {
            var filter = Builders<PropertyImageDataModel>.Filter.And(
                Builders<PropertyImageDataModel>.Filter.Eq(x => x.Id, idProperty),
                Builders<PropertyImageDataModel>.Filter.Eq(x => x.Enabled, true)
            );

            // Return the first enabled image
            var imageDataModel = await _propertyImageCollection.Find(filter).FirstOrDefaultAsync();

            if (imageDataModel == null)
                return null;

            return new PropertyImage
            {
                Id = imageDataModel.Id,
                IdProperty = imageDataModel.IdProperty,
                File = imageDataModel.File,
                Enabled = imageDataModel.Enabled
            };
        }

        public async Task DisableImagesAsync(string propertyId)
        {
            var filter = Builders<PropertyImageDataModel>.Filter.Eq(x => x.IdProperty, propertyId);
            var update = Builders<PropertyImageDataModel>.Update.Set(x => x.Enabled, false);

            await _propertyImageCollection.UpdateManyAsync(filter, update);
        }

        public async Task AddImageAsync(string propertyId, byte[] imageFile)
        {
            var newImage = new PropertyImageDataModel
            {
                IdProperty = propertyId,
                File = imageFile,
                Enabled = true
            };

            await _propertyImageCollection.InsertOneAsync(newImage);
        }

        public async Task DeleteImagesAsync(string propertyId)
        {
            var filter = Builders<PropertyImageDataModel>.Filter.Eq(x => x.IdProperty, propertyId);
            await _propertyImageCollection.DeleteManyAsync(filter);
        }
    }
}
