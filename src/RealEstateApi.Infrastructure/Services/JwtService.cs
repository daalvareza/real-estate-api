using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RealEstateApi.Application.Interfaces;

namespace RealEstateApi.Infrastructure.Services
{
    public class JwtSettings
    {
        public string Key { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int ExpiresMinutes { get; set; }
    }

    public class JwtService : IJwtService
    {
        private readonly JwtSettings _settings;

        public JwtService(JwtSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrEmpty(settings.Key)) throw new ArgumentNullException(nameof(settings.Key), "JWT key cannot be null or empty.");
            if (string.IsNullOrEmpty(settings.Issuer)) throw new ArgumentNullException(nameof(settings.Issuer), "JWT issuer cannot be null or empty.");
            if (string.IsNullOrEmpty(settings.Audience)) throw new ArgumentNullException(nameof(settings.Audience), "JWT audience cannot be null or empty.");
            if (settings.ExpiresMinutes <= 0) throw new ArgumentException("JWT expiration time must be greater than zero.", nameof(settings.ExpiresMinutes));

            _settings = settings;
        }

        public string GenerateToken(string ownerId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, ownerId)
                },
                expires: DateTime.Now.AddMinutes(_settings.ExpiresMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
