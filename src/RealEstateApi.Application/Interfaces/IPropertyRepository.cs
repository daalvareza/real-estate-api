using RealEstateApi.Domain.Entities;
using RealEstateApi.Application.Dtos;

namespace RealEstateApi.Application.Interfaces
{
    public interface IPropertyRepository
    {
        Task<IEnumerable<Property>> GetFilteredPropertiesAsync(PropertyFilterDto filter);

        Task<Property?> GetByIdAsync(string idProperty);

        Task<string> CreateAsync(Property property, byte[] imageFile);

        Task<bool> UpdateAsync(Property property);

        Task<bool> DeleteAsync(string idProperty);
    }
}
