using RealStateApi.Application.Dtos;

namespace RealStateApi.Application.Interfaces
{
    public interface IPropertyService
    {
        Task<IEnumerable<PropertyListDto>> GetFilteredPropertiesAsync(PropertyFilterDto filterDto);

        Task<string> CreatePropertyAsync(PropertyCreateDto createDto);

        Task<bool> UpdatePropertyAsync(PropertyUpdateDto updateDto);

        Task<bool> DeletePropertyAsync(string idProperty, string ownerId);
    }
}