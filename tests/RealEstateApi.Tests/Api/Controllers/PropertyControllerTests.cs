using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RealEstateApi.Api.Controllers;
using RealEstateApi.Application.Interfaces;
using RealEstateApi.Application.Dtos;

namespace RealEstateApi.Tests.Api.Controllers
{
    public class PropertyControllerTests
    {
        private readonly PropertyController _controller;
        private readonly Mock<IPropertyService> _mockPropertyService;

        public PropertyControllerTests()
        {
            _mockPropertyService = new Mock<IPropertyService>();
            _controller = new PropertyController(_mockPropertyService.Object);

            // Mock HttpContext for Authorize attribute and getOwnerId
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "ownerId")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };
        }

        [Fact]
        public async Task Get_ReturnsProperties()
        {
            // Arrange
            var filter = new PropertyFilterDto();
            var properties = new List<PropertyListDto> { new PropertyListDto { IdProperty = "1" } };
            _mockPropertyService.Setup(s => s.GetFilteredPropertiesAsync(filter))
                .ReturnsAsync(properties);

            // Act
            var result = await _controller.Get(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(properties, okResult.Value);
        }

        [Fact]
        public async Task GetPropertyById_ReturnsProperty()
        {
            // Arrange
            var id = "1";
            var properties = new List<PropertyListDto>
            {
                new PropertyListDto { IdProperty = id }
            };

            _mockPropertyService.Setup(s => s.GetFilteredPropertiesAsync(It.IsAny<PropertyFilterDto>()))
                .ReturnsAsync(properties);

            // Act
            var result = await _controller.GetPropertyById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var property = Assert.IsType<PropertyListDto>(okResult.Value);
            Assert.Equal(id, property.IdProperty);
        }

        [Fact]
        public async Task GetPropertyById_ReturnsNotFound_WhenPropertyDoesNotExist()
        {
            // Arrange
            var id = "1";
            _mockPropertyService.Setup(s => s.GetFilteredPropertiesAsync(It.IsAny<PropertyFilterDto>()))
                .ReturnsAsync(new List<PropertyListDto>());

            // Act
            var result = await _controller.GetPropertyById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedProperty()
        {
            // Arrange
            var createDto = new PropertyCreateDto { Name = "Test Property" };
            _mockPropertyService.Setup(s => s.CreatePropertyAsync(createDto))
                .ReturnsAsync("newPropertyId");

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdResult.Value);
            var idProperty = createdResult.Value?.GetType().GetProperty("IdProperty")?.GetValue(createdResult.Value, null);
            Assert.NotNull(idProperty);
            Assert.Equal("newPropertyId", idProperty.ToString());
        }

        [Fact]
        public async Task Update_ReturnsNoContent()
        {
            // Arrange
            var updateDto = new PropertyUpdateDto { IdProperty = "1" };
            _mockPropertyService.Setup(s => s.UpdatePropertyAsync(updateDto))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Update(updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenPropertyNotUpdated()
        {
            // Arrange
            var updateDto = new PropertyUpdateDto { IdProperty = "1" };
            _mockPropertyService.Setup(s => s.UpdatePropertyAsync(updateDto))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Update(updateDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
            var message = notFoundResult.Value?.GetType().GetProperty("Message")?.GetValue(notFoundResult.Value, null);
            Assert.NotNull(message);
            Assert.Equal("Property not found or not updated.", message.ToString());
        }

        [Fact]
        public async Task Delete_ReturnsNoContent()
        {
            // Arrange
            var propertyId = "1";
            _mockPropertyService.Setup(s => s.DeletePropertyAsync(propertyId, "ownerId"))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(propertyId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenPropertyNotDeleted()
        {
            // Arrange
            var propertyId = "1";
            _mockPropertyService.Setup(s => s.DeletePropertyAsync(propertyId, "ownerId"))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(propertyId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
            var message = notFoundResult.Value?.GetType().GetProperty("Message")?.GetValue(notFoundResult.Value, null);
            Assert.NotNull(message);
            Assert.Equal("Property not found or not deleted.", message.ToString());
        }
    }
}
