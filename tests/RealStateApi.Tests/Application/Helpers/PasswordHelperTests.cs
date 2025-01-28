using Xunit;
using RealStateApi.Application.Common.Helpers;

public class PasswordHelperTests
{
    [Fact]
    public void HashPassword_ReturnsHashAndSalt()
    {
        // Arrange
        string password = "testpassword";

        // Act
        var result = PasswordHelper.HashPassword(password);

        // Assert
        Assert.NotNull(result.Hash);
        Assert.NotNull(result.Salt);
        Assert.NotEmpty(result.Hash);
        Assert.NotEmpty(result.Salt);
    }

    [Fact]
    public void VerifyPassword_ReturnsTrue_ForCorrectPassword()
    {
        // Arrange
        string password = "testpassword";
        var (hash, salt) = PasswordHelper.HashPassword(password);

        // Act
        bool isValid = PasswordHelper.VerifyPassword(password, hash, salt);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_ForIncorrectPassword()
    {
        // Arrange
        string password = "testpassword";
        string wrongPassword = "wrongpassword";
        var (hash, salt) = PasswordHelper.HashPassword(password);

        // Act
        bool isValid = PasswordHelper.VerifyPassword(wrongPassword, hash, salt);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_ForInvalidSaltOrHash()
    {
        // Arrange
        string password = "testpassword";
        string invalidHash = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 });
        string invalidSalt = Convert.ToBase64String(new byte[] { 6, 7, 8, 9, 10 });

        // Act
        bool isValid = PasswordHelper.VerifyPassword(password, invalidHash, invalidSalt);

        // Assert
        Assert.False(isValid);
    }
}
