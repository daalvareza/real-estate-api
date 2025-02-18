using RealEstateApi.Domain.Entities;

namespace RealEstateApi.Application.Interfaces
{
    public interface IPropertyImageRepository
    {
        Task<PropertyImage?> GetFirstImageAsync(string idProperty);
        Task DisableImagesAsync(string propertyId);
        Task AddImageAsync(string propertyId, byte[] imageFile);
        Task DeleteImagesAsync(string propertyId);
    }
}
