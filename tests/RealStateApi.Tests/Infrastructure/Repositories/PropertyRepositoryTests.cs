using Moq;
using MongoDB.Driver;
using MongoDB.Bson;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using RealStateApi.Domain.Entities;
using RealStateApi.Infrastructure.DataModels;
using RealStateApi.Infrastructure.Data;
using RealStateApi.Infrastructure.Repositories;
using RealStateApi.Application.Interfaces;
using RealStateApi.Application.Dtos;

namespace RealStateApi.Tests.Infrastructure.Repositories
{
    public class PropertyRepositoryTests
    {
        private readonly Mock<IMongoDbContext> _mockMongoDbContext;
        private readonly Mock<IMongoCollection<PropertyDataModel>> _mockPropertyCollection;
        private readonly Mock<IMongoCollection<PropertyImageDataModel>> _mockPropertyImageCollection;
        private readonly PropertyRepository _propertyRepository;

        public PropertyRepositoryTests()
        {
            _mockMongoDbContext = new Mock<IMongoDbContext>();

            _mockPropertyCollection = new Mock<IMongoCollection<PropertyDataModel>>();
            _mockPropertyImageCollection = new Mock<IMongoCollection<PropertyImageDataModel>>();

            _mockMongoDbContext
                .Setup(x => x.GetCollection<PropertyDataModel>("Property"))
                .Returns(_mockPropertyCollection.Object);

            _mockMongoDbContext
                .Setup(x => x.GetCollection<PropertyImageDataModel>("PropertyImage"))
                .Returns(_mockPropertyImageCollection.Object);

            _propertyRepository = new PropertyRepository(_mockMongoDbContext.Object);
        }

        [Fact]
        public async Task GetFilteredPropertiesAsync_ReturnsCorrectProperties()
        {
            // Arrange
            var filterDto = new PropertyFilterDto
            {
                Name = "Test",
                Address = "Ave",
                MinPrice = 1000,
                MaxPrice = 3000,
                Page = 1,
                PageSize = 10
            };

            var propertyDataList = new List<PropertyDataModel>
            {
                new PropertyDataModel
                {
                    Id = "prop1",
                    Name = "Test Property",
                    Address = "123 Ave",
                    Price = 2000,
                    CodeInternal = "1",
                    Year = 2020,
                    IdOwner = "owner1"
                }
            };

            var mockCursor = new Mock<IAsyncCursor<PropertyDataModel>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(propertyDataList);

            _mockPropertyCollection
                .Setup(_ => _.FindAsync(
                    It.IsAny<FilterDefinition<PropertyDataModel>>(),
                    It.IsAny<FindOptions<PropertyDataModel, PropertyDataModel>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _propertyRepository.GetFilteredPropertiesAsync(filterDto);

            // Assert
            Assert.NotNull(result);
            var properties = new List<Property>(result);
            Assert.Single(properties);
            Assert.Equal("prop1", properties[0].Id);
            Assert.Equal("Test Property", properties[0].Name);
            Assert.Equal("123 Ave", properties[0].Address);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsProperty_WhenExists()
        {
            // Arrange
            var propertyId = "prop123";
            var propertyDataModel = new PropertyDataModel
            {
                Id = propertyId,
                Name = "Sample Property",
                Address = "456 Road",
                Price = 1500,
                CodeInternal = "5",
                Year = 2021,
                IdOwner = "owner123"
            };

            var mockCursor = new Mock<IAsyncCursor<PropertyDataModel>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<PropertyDataModel> { propertyDataModel });

            _mockPropertyCollection
                .Setup(_ => _.FindAsync(
                    It.IsAny<FilterDefinition<PropertyDataModel>>(),
                    It.IsAny<FindOptions<PropertyDataModel, PropertyDataModel>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _propertyRepository.GetByIdAsync(propertyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(propertyId, result.Id);
            Assert.Equal("Sample Property", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenPropertyDoesNotExist()
        {
            // Arrange
            var propertyId = "missingProp";

            var mockCursor = new Mock<IAsyncCursor<PropertyDataModel>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);

            _mockPropertyCollection
                .Setup(_ => _.FindAsync(
                    It.IsAny<FilterDefinition<PropertyDataModel>>(),
                    It.IsAny<FindOptions<PropertyDataModel, PropertyDataModel>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _propertyRepository.GetByIdAsync(propertyId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreatePropertyAndImage()
        {
            // Arrange
            var property = new Property
            {
                Id = null, // Will be assigned in the method (CodeInternal)
                Name = "New Property",
                Address = "789 Blvd",
                Price = 1200,
                IdOwner = "ownerX"
            };
            var imageData = new byte[] { 1, 2, 3 };

            var propertyDataModel = new PropertyDataModel
            {
                Id = "propNew",
                Name = "New Property",
                Address = "789 Blvd",
                Price = 1200,
                CodeInternal = "99",
                Year = 0,
                IdOwner = "ownerX"
            };

            // Mock the CountDocumentsAsync to simulate existing property count
            _mockPropertyCollection
                .Setup(_ => _.CountDocumentsAsync(
                    It.IsAny<FilterDefinition<PropertyDataModel>>(),
                    It.IsAny<CountOptions>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(98); // So CodeInternal becomes "99"

            // Mock InsertOneAsync for Property
            _mockPropertyCollection
                .Setup(_ => _.InsertOneAsync(
                    It.IsAny<PropertyDataModel>(),
                    null,
                    It.IsAny<CancellationToken>()
                ))
                .Callback<PropertyDataModel, InsertOneOptions, CancellationToken>((p, o, t) =>
                {
                    // Assign an Id after insert simulation
                    p.Id = "propNew";
                })
                .Returns(Task.CompletedTask);

            // Mock InsertOneAsync for PropertyImage
            _mockPropertyImageCollection
                .Setup(_ => _.InsertOneAsync(
                    It.IsAny<PropertyImageDataModel>(),
                    null,
                    It.IsAny<CancellationToken>()
                ))
                .Returns(Task.CompletedTask);

            // Act
            var resultId = await _propertyRepository.CreateAsync(property, imageData);

            // Assert
            // Ensure the final ID matches what we assigned in the Callback
            Assert.Equal("propNew", resultId);
            _mockPropertyCollection.Verify(_ => _.InsertOneAsync(
                It.IsAny<PropertyDataModel>(),
                null,
                It.IsAny<CancellationToken>()), Times.Once);

            _mockPropertyImageCollection.Verify(_ => _.InsertOneAsync(
                It.IsAny<PropertyImageDataModel>(),
                null,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsTrue_WhenPropertyIsUpdated()
        {
            // Arrange
            var property = new Property
            {
                Id = "propUpdate",
                Name = "Updated Property"
            };

            var mockReplaceResult = new Mock<ReplaceOneResult>();
            mockReplaceResult.Setup(_ => _.ModifiedCount).Returns(1);

            _mockPropertyCollection
                .Setup(_ => _.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<PropertyDataModel>>(),
                    It.IsAny<PropertyDataModel>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockReplaceResult.Object);

            // Act
            var result = await _propertyRepository.UpdateAsync(property);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsFalse_WhenNoPropertyUpdated()
        {
            // Arrange
            var property = new Property
            {
                Id = "propMissing",
                Name = "Missing Property"
            };

            var mockReplaceResult = new Mock<ReplaceOneResult>();
            mockReplaceResult.Setup(_ => _.ModifiedCount).Returns(0);

            _mockPropertyCollection
                .Setup(_ => _.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<PropertyDataModel>>(),
                    It.IsAny<PropertyDataModel>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockReplaceResult.Object);

            // Act
            var result = await _propertyRepository.UpdateAsync(property);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_WhenDeleted()
        {
            // Arrange
            var propertyId = "propDel";
            var mockDeleteResult = new Mock<DeleteResult>();
            mockDeleteResult.Setup(_ => _.DeletedCount).Returns(1);

            _mockPropertyCollection
                .Setup(_ => _.DeleteOneAsync(
                    It.IsAny<FilterDefinition<PropertyDataModel>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockDeleteResult.Object);

            // Act
            var result = await _propertyRepository.DeleteAsync(propertyId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotDeleted()
        {
            // Arrange
            var propertyId = "propDel";
            var mockDeleteResult = new Mock<DeleteResult>();
            mockDeleteResult.Setup(_ => _.DeletedCount).Returns(0);

            _mockPropertyCollection
                .Setup(_ => _.DeleteOneAsync(
                    It.IsAny<FilterDefinition<PropertyDataModel>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockDeleteResult.Object);

            // Act
            var result = await _propertyRepository.DeleteAsync(propertyId);

            // Assert
            Assert.False(result);
        }
    }
}
