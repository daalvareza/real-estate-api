using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApi.Domain.Entities;
using RealEstateApi.Infrastructure.Data;
using RealEstateApi.Application.Interfaces;
using RealEstateApi.Application.Dtos;
using RealEstateApi.Infrastructure.DataModels;

namespace RealEstateApi.Infrastructure.Repositories
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly IMongoCollection<PropertyDataModel> _propertyCollection;
        private readonly IMongoCollection<PropertyImageDataModel> _propertyImageCollection;

        public PropertyRepository(IMongoDbContext context)
        {
            _propertyCollection = context.GetCollection<PropertyDataModel>("Property");
            _propertyImageCollection = context.GetCollection<PropertyImageDataModel>("PropertyImage");
        }

        public async Task<IEnumerable<Property>> GetFilteredPropertiesAsync(PropertyFilterDto filterDto)
        {
            var builder = Builders<PropertyDataModel>.Filter;
            var filters = new List<FilterDefinition<PropertyDataModel>>();

            if (!string.IsNullOrEmpty(filterDto.Name))
            {
                filters.Add(builder.Regex(x => x.Name, new BsonRegularExpression(filterDto.Name, "i")));
            }
            if (!string.IsNullOrEmpty(filterDto.Address))
            {
                filters.Add(builder.Regex(x => x.Address, new BsonRegularExpression(filterDto.Address, "i")));
            }
            if (filterDto.MinPrice.HasValue)
            {
                filters.Add(builder.Gte(x => x.Price, filterDto.MinPrice.Value));
            }
            if (filterDto.MaxPrice.HasValue)
            {
                filters.Add(builder.Lte(x => x.Price, filterDto.MaxPrice.Value));
            }

            var finalFilter = filters.Count > 0
                ? builder.And(filters)
                : builder.Empty;

            int skip = (filterDto.Page - 1) * filterDto.PageSize;
            int limit = filterDto.PageSize;

            var results = await _propertyCollection.Find(finalFilter)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();

            return results.Select(ToDomainProperty).ToList();
        }

        public async Task<Property?> GetByIdAsync(string id)
        {
            var filter = Builders<PropertyDataModel>.Filter.Eq(x => x.Id, id);
            var propertyDataModel = await _propertyCollection.Find(filter).FirstOrDefaultAsync();

            if (propertyDataModel == null)
                return null;

            return ToDomainProperty(propertyDataModel);
        }

        public async Task<string> CreateAsync(Property entity, byte[] imageFile)
        {
            // Generate CodeInternal
            var totalProperties = await _propertyCollection.CountDocumentsAsync(FilterDefinition<PropertyDataModel>.Empty);
            entity.CodeInternal = (totalProperties + 1).ToString();

            var propertyDataModel = ToDataModel(entity);
            await _propertyCollection.InsertOneAsync(propertyDataModel);

            // Create Image Entry
            var propertyImage = new PropertyImageDataModel
            {
                IdProperty = propertyDataModel.Id,
                File = imageFile,
                Enabled = true
            };
            await _propertyImageCollection.InsertOneAsync(propertyImage);

            return propertyDataModel.Id;
        }

        public async Task<bool> UpdateAsync(Property entity)
        {
            var propertyDataModel = ToDataModel(entity);
            var filter = Builders<PropertyDataModel>.Filter.Eq(x => x.Id, propertyDataModel.Id);

            var result = await _propertyCollection.ReplaceOneAsync(filter, propertyDataModel);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var filter = Builders<PropertyDataModel>.Filter.Eq(x => x.Id, id);
            var result = await _propertyCollection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        // -- MAPPING HELPERS --

        private static PropertyDataModel ToDataModel(Property property)
        {
            return new PropertyDataModel
            {
                Id = property.Id,
                Name = property.Name,
                Address = property.Address,
                Price = property.Price,
                CodeInternal = property.CodeInternal,
                Year = property.Year,
                IdOwner = property.IdOwner
            };
        }

        private static Property ToDomainProperty(PropertyDataModel dataModel)
        {
            return new Property
            {
                Id = dataModel.Id,
                Name = dataModel.Name,
                Address = dataModel.Address,
                Price = dataModel.Price,
                CodeInternal = dataModel.CodeInternal,
                Year = dataModel.Year,
                IdOwner = dataModel.IdOwner
            };
        }
    }
}
