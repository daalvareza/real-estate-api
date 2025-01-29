using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using RealEstateApi.Application.Interfaces;
using RealEstateApi.Application.Services;
using RealEstateApi.Domain.Entities;
using RealEstateApi.Application.Dtos;

namespace RealEstateApi.Tests.Application.Services
{
    public class PropertyServiceTests
    {
        private readonly Mock<IPropertyRepository> _mockPropertyRepository;
        private readonly Mock<IOwnerRepository> _mockOwnerRepository;
        private readonly Mock<IPropertyImageRepository> _mockPropertyImageRepository;
        private readonly PropertyService _propertyService;

        public PropertyServiceTests()
        {
            _mockPropertyRepository = new Mock<IPropertyRepository>();
            _mockOwnerRepository = new Mock<IOwnerRepository>();
            _mockPropertyImageRepository = new Mock<IPropertyImageRepository>();
            _propertyService = new PropertyService(
                _mockPropertyRepository.Object,
                _mockOwnerRepository.Object,
                _mockPropertyImageRepository.Object
            );
        }

        [Fact]
        public async Task GetFilteredPropertiesAsync_ReturnsPropertyList()
        {
            // Arrange
            var filterDto = new PropertyFilterDto { Name = "Test" };
            var properties = new List<Property>
            {
                new Property { Id = "1", Name = "Test Property", IdOwner = "owner1" }
            };
            var owner = new Owner { Id = "owner1", Name = "Owner Name" };
            var image = new PropertyImage { Id = "image1", File = new byte[] { 1, 2, 3 } };

            _mockPropertyRepository.Setup(repo => repo.GetFilteredPropertiesAsync(filterDto))
                                .ReturnsAsync((properties, 1));
            _mockOwnerRepository.Setup(repo => repo.GetOwnerByIdAsync("owner1"))
                                .ReturnsAsync(owner);
            _mockPropertyImageRepository.Setup(repo => repo.GetFirstImageAsync("1"))
                                        .ReturnsAsync(image);

            // Act
            var result = await _propertyService.GetFilteredPropertiesAsync(filterDto);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Properties);
            Assert.Equal("Test Property", result.Properties.First().Name);
            Assert.Equal("Owner Name", result.Properties.First().OwnerName);
            Assert.Equal(Convert.ToBase64String(new byte[] { 1, 2, 3 }), result.Properties.First().FirstImage);
        }

        [Fact]
        public async Task CreatePropertyAsync_ReturnsPropertyId()
        {
            // Arrange
            var createDto = new PropertyCreateDto
            {
                Name = "New Property",
                IdOwner = "owner1",
                Image = Mock.Of<IFormFile>()
            };
            var owner = new Owner { Id = "owner1", Name = "Owner Name" };

            _mockOwnerRepository.Setup(repo => repo.GetOwnerByIdAsync("owner1"))
                                .ReturnsAsync(owner);
            _mockPropertyRepository.Setup(repo => repo.CreateAsync(It.IsAny<Property>(), It.IsAny<byte[]>()))
                                .ReturnsAsync("newPropertyId");

            // Act
            var result = await _propertyService.CreatePropertyAsync(createDto);

            // Assert
            Assert.Equal("newPropertyId", result);
        }

        [Fact]
        public async Task UpdatePropertyAsync_UpdatesProperty()
        {
            // Arrange
            var updateDto = new PropertyUpdateDto
            {
                IdProperty = "1",
                IdOwner = "owner1",
                Name = "Updated Name"
            };
            var property = new Property
            {
                Id = "1",
                Name = "Original Name",
                IdOwner = "owner1"
            };

            _mockPropertyRepository.Setup(repo => repo.GetByIdAsync("1"))
                                .ReturnsAsync(property);
            _mockPropertyRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Property>()))
                                .ReturnsAsync(true);

            // Act
            var result = await _propertyService.UpdatePropertyAsync(updateDto);

            // Assert
            Assert.True(result);
            _mockPropertyRepository.Verify(repo => repo.UpdateAsync(It.Is<Property>(p => p.Name == "Updated Name")));
        }

        [Fact]
        public async Task DeletePropertyAsync_DeletesProperty()
        {
            // Arrange
            var property = new Property
            {
                Id = "1",
                IdOwner = "owner1"
            };

            _mockPropertyRepository.Setup(repo => repo.GetByIdAsync("1"))
                                .ReturnsAsync(property);
            _mockPropertyRepository.Setup(repo => repo.DeleteAsync("1"))
                                .ReturnsAsync(true);

            // Act
            var result = await _propertyService.DeletePropertyAsync("1", "owner1");

            // Assert
            Assert.True(result);
            _mockPropertyImageRepository.Verify(repo => repo.DeleteImagesAsync("1"), Times.Once);
        }

        [Fact]
        public async Task CreatePropertyAsync_ThrowsException_WhenOwnerDoesNotExist()
        {
            // Arrange
            var createDto = new PropertyCreateDto { IdOwner = "invalidOwner" };
            _mockOwnerRepository.Setup(repo => repo.GetOwnerByIdAsync("invalidOwner"))
                                .ReturnsAsync((Owner?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _propertyService.CreatePropertyAsync(createDto));
        }
    }
}
