using Moq;
using MongoDB.Driver;
using RealStateApi.Domain.Entities;
using RealStateApi.Infrastructure.Data;
using RealStateApi.Infrastructure.Repositories;
using Xunit;
using System.Threading.Tasks;

namespace RealStateApi.Tests.Infrastructure.Repositories
{
    public class OwnerRepositoryTests
    {
        private readonly Mock<IMongoDbContext> _mockMongoDbContext;
        private readonly Mock<IMongoCollection<Owner>> _mockOwnerCollection;
        private readonly OwnerRepository _ownerRepository;

        public OwnerRepositoryTests()
        {
            _mockMongoDbContext = new Mock<IMongoDbContext>();
            _mockOwnerCollection = new Mock<IMongoCollection<Owner>>();

            _mockMongoDbContext
                .Setup(x => x.GetCollection<Owner>(It.IsAny<string>()))
                .Returns(_mockOwnerCollection.Object);

            _ownerRepository = new OwnerRepository(_mockMongoDbContext.Object);
        }

        [Fact]
        public async Task GetOwnerByIdAsync_ShouldReturnOwner_WhenOwnerExists()
        {
            // Arrange
            var ownerId = "123";
            var owner = new Owner { Id = ownerId, Email = "john@example.com" };

            // Mock the cursor behavior to simulate a successful query
            var mockCursor = new Mock<IAsyncCursor<Owner>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<Owner> { owner });
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);

            _mockOwnerCollection
                .Setup(_ => _.FindAsync(
                    It.IsAny<FilterDefinition<Owner>>(),
                    It.IsAny<FindOptions<Owner, Owner>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _ownerRepository.GetOwnerByIdAsync(ownerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ownerId, result.Id);
        }

        [Fact]
        public async Task GetOwnerByIdAsync_ShouldReturnNull_WhenOwnerDoesNotExist()
        {
            // Arrange
            var ownerId = "123";

            // Mock the cursor to return no results
            var mockCursor = new Mock<IAsyncCursor<Owner>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<Owner>());
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);

            _mockOwnerCollection
                .Setup(_ => _.FindAsync(
                    It.IsAny<FilterDefinition<Owner>>(),
                    It.IsAny<FindOptions<Owner, Owner>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _ownerRepository.GetOwnerByIdAsync(ownerId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateOwnerAsync_ShouldInsertOwner()
        {
            // Arrange
            var owner = new Owner { Id = "123", Email = "john@example.com" };

            // Act
            await _ownerRepository.CreateOwnerAsync(owner);

            // Assert
            _mockOwnerCollection.Verify(
                _ => _.InsertOneAsync(
                    owner,
                    null,
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );
        }
    }
}