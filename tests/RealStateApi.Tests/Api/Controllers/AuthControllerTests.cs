using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using RealStateApi.Api.Controllers;
using RealStateApi.Application.Interfaces;
using RealStateApi.Application.Common.Helpers;
using RealStateApi.Domain.Entities;
using System.Threading.Tasks;

public class AuthControllerTests
{
    private readonly Mock<IOwnerRepository> _mockOwnerRepository;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockOwnerRepository = new Mock<IOwnerRepository>();
        _mockJwtService = new Mock<IJwtService>();
        _controller = new AuthController(_mockOwnerRepository.Object, _mockJwtService.Object);
    }

    [Fact]
    public async Task Login_InvalidEmail_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "invalid@test.com", Password = "password" };
        _mockOwnerRepository.Setup(repo => repo.GetOwnerByEmailAsync(loginRequest.Email))
                            .ReturnsAsync((Owner?)null);

        // Act
        var result = await _controller.Login(loginRequest) as UnauthorizedObjectResult;

        // Assert
        Assert.Equal(401, result.StatusCode);
        Assert.NotNull(result.Value);
        var value = result.Value.GetType().GetProperty("Message")?.GetValue(result.Value, null);
        Assert.NotNull(value);
        Assert.Equal("Invalid email or password.", value.ToString());
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "valid@test.com", Password = "wrongpassword" };
        var owner = new Owner
        {
            Id = "ownerId",
            Email = "valid@test.com",
            PasswordHash = PasswordHelper.HashPassword("correctpassword").Hash,
            PasswordSalt = PasswordHelper.HashPassword("correctpassword").Salt
        };

        _mockOwnerRepository.Setup(repo => repo.GetOwnerByEmailAsync(loginRequest.Email))
                            .ReturnsAsync(owner);

        // Act
        var result = await _controller.Login(loginRequest) as UnauthorizedObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(401, result.StatusCode);
        Assert.NotNull(result.Value);

        var value = result.Value.GetType().GetProperty("Message")?.GetValue(result.Value, null);
        Assert.NotNull(value);
        Assert.Equal("Invalid email or password.", value.ToString());
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "valid@test.com", Password = "correctpassword" };
        var hashedPassword = PasswordHelper.HashPassword("correctpassword");

        var owner = new Owner
        {
            Id = "ownerId",
            Email = "valid@test.com",
            PasswordHash = hashedPassword.Hash,
            PasswordSalt = hashedPassword.Salt
        };

        _mockOwnerRepository.Setup(repo => repo.GetOwnerByEmailAsync(loginRequest.Email))
                            .ReturnsAsync(owner);

        _mockJwtService.Setup(jwt => jwt.GenerateToken(owner.Id))
                    .Returns("mockToken");

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var value = okResult.Value.GetType().GetProperty("Token")?.GetValue(okResult.Value, null);
        Assert.NotNull(value);
        Assert.Equal("mockToken", value.ToString());
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsSuccessMessage()
    {
        // Arrange
        var registerRequest = new LoginRequest { Email = "newuser@test.com", Password = "newpassword" };
        _mockOwnerRepository.Setup(repo => repo.CreateOwnerAsync(It.IsAny<Owner>()))
                            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Register(registerRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value.GetType().GetProperty("Message")?.GetValue(okResult.Value, null);
        Assert.NotNull(value);
        Assert.Equal("Registration successful", value.ToString());
    }
}
