using RealEstateApi.Application.Dtos;

namespace RealEstateApi.Application.Interfaces
{
    public interface IPropertyService
    {
        Task<PaginatedPropertiesDto> GetFilteredPropertiesAsync(PropertyFilterDto filterDto);

        Task<string> CreatePropertyAsync(PropertyCreateDto createDto);

        Task<bool> UpdatePropertyAsync(PropertyUpdateDto updateDto);

        Task<bool> DeletePropertyAsync(string idProperty, string ownerId);
    }
}