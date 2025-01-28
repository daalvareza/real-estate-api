using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Xunit;
using RealStateApi.Infrastructure.Services;

public class JwtServiceTests
{
    private readonly JwtSettings _settings;
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        _settings = new JwtSettings
        {
            Key = "ThisIsASecretKeyForJwtTesting123!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiresMinutes = 60
        };

        _jwtService = new JwtService(_settings);
    }

    [Fact]
    public void GenerateToken_ReturnsValidJwtToken()
    {
        // Arrange
        string ownerId = Guid.NewGuid().ToString();

        // Act
        string token = _jwtService.GenerateToken(ownerId);

        // Assert
        Assert.NotNull(token);

        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(token));

        var jwtToken = handler.ReadJwtToken(token);
        Assert.Equal(_settings.Issuer, jwtToken.Issuer);
        Assert.Equal(_settings.Audience, jwtToken.Audiences.FirstOrDefault());
        Assert.Equal(ownerId, jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
    }

    [Fact]
    public void GenerateToken_ThrowsException_WhenSettingsAreInvalid()
    {
        // Arrange
        var invalidSettings = new JwtSettings
        {
            Key = null!,
            Issuer = null!,
            Audience = null!,
            ExpiresMinutes = 0
        };

        // Act & Assert for Key
        var keyException = Assert.Throws<ArgumentNullException>(() => new JwtService(new JwtSettings
        {
            Key = null!,
            Issuer = "Issuer",
            Audience = "Audience",
            ExpiresMinutes = 60
        }));
        Assert.Equal("Key", keyException.ParamName);

        // Act & Assert for Issuer
        var issuerException = Assert.Throws<ArgumentNullException>(() => new JwtService(new JwtSettings
        {
            Key = "ValidKey1234567890123456789012345678",
            Issuer = null!,
            Audience = "Audience",
            ExpiresMinutes = 60
        }));
        Assert.Equal("Issuer", issuerException.ParamName);

        // Act & Assert for Audience
        var audienceException = Assert.Throws<ArgumentNullException>(() => new JwtService(new JwtSettings
        {
            Key = "ValidKey1234567890123456789012345678",
            Issuer = "Issuer",
            Audience = null!,
            ExpiresMinutes = 60
        }));
        Assert.Equal("Audience", audienceException.ParamName);

        // Act & Assert for ExpiresMinutes
        var expiresException = Assert.Throws<ArgumentException>(() => new JwtService(new JwtSettings
        {
            Key = "ValidKey1234567890123456789012345678",
            Issuer = "Issuer",
            Audience = "Audience",
            ExpiresMinutes = 0
        }));
        Assert.Equal("ExpiresMinutes", expiresException.ParamName);
        Assert.Contains("JWT expiration time must be greater than zero.", expiresException.Message);
    }

    [Fact]
    public void GenerateToken_ThrowsException_WhenKeyIsInvalid()
    {
        // Arrange
        var invalidSettings = new JwtSettings
        {
            Key = "ShortKey",
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            ExpiresMinutes = _settings.ExpiresMinutes
        };

        var jwtService = new JwtService(invalidSettings);

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => jwtService.GenerateToken(Guid.NewGuid().ToString()));
        Assert.Contains("IDX10653", exception.Message);
        Assert.Contains("HS256", exception.Message);
    }
}
