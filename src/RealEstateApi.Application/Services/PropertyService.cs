using Microsoft.AspNetCore.Http;
using RealEstateApi.Application.Dtos;
using RealEstateApi.Application.Interfaces;
using RealEstateApi.Domain.Entities;

namespace RealEstateApi.Application.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IOwnerRepository _ownerRepository;
        private readonly IPropertyImageRepository _propertyImageRepository;

        public PropertyService(
            IPropertyRepository propertyRepository,
            IOwnerRepository ownerRepository,
            IPropertyImageRepository propertyImageRepository)
        {
            _propertyRepository = propertyRepository;
            _ownerRepository = ownerRepository;
            _propertyImageRepository = propertyImageRepository;
        }

        public async Task<IEnumerable<PropertyListDto>> GetFilteredPropertiesAsync(PropertyFilterDto filterDto)
        {
            var properties = await _propertyRepository.GetFilteredPropertiesAsync(filterDto);
            var result = new List<PropertyListDto>();

            foreach (var property in properties)
            {
                var owner = await _ownerRepository.GetOwnerByIdAsync(property.IdOwner);
                var imageEntity = await _propertyImageRepository.GetFirstImageAsync(property.Id);

                var base64Image = imageEntity is not null
                    ? Convert.ToBase64String(imageEntity.File)
                    : null;

                result.Add(new PropertyListDto
                {
                    IdProperty = property.Id,
                    IdOwner = property.IdOwner,
                    OwnerName = owner?.Name ?? string.Empty,
                    Name = property.Name,
                    Address = property.Address,
                    Price = property.Price,
                    FirstImage = base64Image
                });
            }

            return result;
        }


        public async Task<string> CreatePropertyAsync(PropertyCreateDto createDto)
        {
            var owner = await _ownerRepository.GetOwnerByIdAsync(createDto.IdOwner);
            if (owner == null)
                throw new ArgumentException("Owner does not exist.");

            var property = new Property
            {
                Name = createDto.Name,
                Address = createDto.Address,
                Price = createDto.Price,
                CodeInternal = createDto.CodeInternal,
                Year = createDto.Year,
                IdOwner = createDto.IdOwner
            };

            var imageFile = await readImageFile(createDto.Image);
            return await _propertyRepository.CreateAsync(property, imageFile);
        }

        public async Task<bool> UpdatePropertyAsync(PropertyUpdateDto updateDto)
        {
            var property = await _propertyRepository.GetByIdAsync(updateDto.IdProperty);
            if (property == null || property.IdOwner != updateDto.IdOwner)
                throw new ArgumentException("Property not found or does not belong to the logged-in owner");

            if (!string.IsNullOrEmpty(updateDto.Name)) property.Name = updateDto.Name;
            if (!string.IsNullOrEmpty(updateDto.Address)) property.Address = updateDto.Address;
            if (updateDto.Price.HasValue) property.Price = updateDto.Price.Value;
            if (updateDto.Year.HasValue) property.Year = updateDto.Year.Value;

            if (updateDto.Image != null)
            {
                await _propertyImageRepository.DisableImagesAsync(property.Id);
                var imageFile = await readImageFile(updateDto.Image);
                await _propertyImageRepository.AddImageAsync(property.Id, imageFile);
            }

            return await _propertyRepository.UpdateAsync(property);
        }

        public async Task<bool> DeletePropertyAsync(string idProperty, string ownerId)
        {
            var property = await _propertyRepository.GetByIdAsync(idProperty);
            if (property == null || property.IdOwner != ownerId)
                throw new ArgumentException("Property not found or does not belong to the logged-in owner");

            // Delete all related images
            await _propertyImageRepository.DeleteImagesAsync(property.Id);

            return await _propertyRepository.DeleteAsync(idProperty);
        }

        private async Task<byte[]> readImageFile(IFormFile image)
        {
            using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
