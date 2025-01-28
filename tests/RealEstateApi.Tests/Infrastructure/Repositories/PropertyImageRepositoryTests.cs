using Moq;
using MongoDB.Driver;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using RealEstateApi.Domain.Entities;
using RealEstateApi.Infrastructure.Data;
using RealEstateApi.Infrastructure.DataModels;
using RealEstateApi.Infrastructure.Repositories;
using RealEstateApi.Application.Interfaces;

namespace RealEstateApi.Tests.Infrastructure.Repositories
{
    public class PropertyImageRepositoryTests
    {
        private readonly Mock<IMongoDbContext> _mockMongoDbContext;
        private readonly Mock<IMongoCollection<PropertyImageDataModel>> _mockPropertyImageCollection;
        private readonly PropertyImageRepository _repository;

        public PropertyImageRepositoryTests()
        {
            _mockMongoDbContext = new Mock<IMongoDbContext>();
            _mockPropertyImageCollection = new Mock<IMongoCollection<PropertyImageDataModel>>();

            _mockMongoDbContext
                .Setup(x => x.GetCollection<PropertyImageDataModel>("PropertyImage"))
                .Returns(_mockPropertyImageCollection.Object);

            _repository = new PropertyImageRepository(_mockMongoDbContext.Object);
        }

        [Fact]
        public async Task GetFirstImageAsync_ReturnsImage_WhenExistsAndEnabled()
        {
            // Arrange
            var propertyId = "imageProp1";
            var imageDataModel = new PropertyImageDataModel
            {
                Id = propertyId,
                IdProperty = "prop1",
                File = new byte[] { 1, 2, 3 },
                Enabled = true
            };

            // Mock cursor simulating a single result
            var mockCursor = new Mock<IAsyncCursor<PropertyImageDataModel>>();
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor
                .Setup(_ => _.Current)
                .Returns(new List<PropertyImageDataModel> { imageDataModel });

            _mockPropertyImageCollection
                .Setup(_ => _.FindAsync(
                    It.IsAny<FilterDefinition<PropertyImageDataModel>>(),
                    It.IsAny<FindOptions<PropertyImageDataModel, PropertyImageDataModel>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetFirstImageAsync(propertyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(propertyId, result.Id);
            Assert.Equal("prop1", result.IdProperty);
            Assert.True(result.Enabled);
            Assert.Equal(new byte[] { 1, 2, 3 }, result.File);
        }

        [Fact]
        public async Task GetFirstImageAsync_ReturnsNull_WhenNoMatchOrDisabled()
        {
            // Arrange
            var mockCursor = new Mock<IAsyncCursor<PropertyImageDataModel>>();
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            mockCursor
                .Setup(_ => _.Current)
                .Returns(new List<PropertyImageDataModel>());

            _mockPropertyImageCollection
                .Setup(_ => _.FindAsync(
                    It.IsAny<FilterDefinition<PropertyImageDataModel>>(),
                    It.IsAny<FindOptions<PropertyImageDataModel, PropertyImageDataModel>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetFirstImageAsync("nonexistentId");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DisableImagesAsync_SetsEnabledFalse()
        {
            // Arrange
            var propertyId = "prop123";
            var updateResult = new Mock<UpdateResult>();
            updateResult.Setup(_ => _.ModifiedCount).Returns(2);

            _mockPropertyImageCollection
                .Setup(_ => _.UpdateManyAsync(
                    It.IsAny<FilterDefinition<PropertyImageDataModel>>(),
                    It.IsAny<UpdateDefinition<PropertyImageDataModel>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateResult.Object);

            // Act
            await _repository.DisableImagesAsync(propertyId);

            // Assert
            _mockPropertyImageCollection.Verify(_ => _.UpdateManyAsync(
                It.IsAny<FilterDefinition<PropertyImageDataModel>>(),
                It.IsAny<UpdateDefinition<PropertyImageDataModel>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AddImageAsync_InsertsNewImage()
        {
            // Arrange
            var propertyId = "propABC";
            var imageBytes = new byte[] { 10, 20, 30 };

            _mockPropertyImageCollection
                .Setup(_ => _.InsertOneAsync(
                    It.IsAny<PropertyImageDataModel>(),
                    null,
                    It.IsAny<CancellationToken>()
                ))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _repository.AddImageAsync(propertyId, imageBytes);

            // Assert
            _mockPropertyImageCollection.Verify(_ => _.InsertOneAsync(
                It.Is<PropertyImageDataModel>(p => p.IdProperty == propertyId && p.File == imageBytes && p.Enabled),
                null,
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteImagesAsync_RemovesImages()
        {
            // Arrange
            var propertyId = "propToDelete";
            var deleteResult = new Mock<DeleteResult>();
            deleteResult.Setup(_ => _.DeletedCount).Returns(3);

            _mockPropertyImageCollection
                .Setup(_ => _.DeleteManyAsync(
                    It.IsAny<FilterDefinition<PropertyImageDataModel>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResult.Object);

            // Act
            await _repository.DeleteImagesAsync(propertyId);

            // Assert
            _mockPropertyImageCollection.Verify(_ => _.DeleteManyAsync(
                It.IsAny<FilterDefinition<PropertyImageDataModel>>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
