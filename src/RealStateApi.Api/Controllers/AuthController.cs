using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using MongoDB.Driver;
using RealStateApi.Application.Interfaces;
using RealStateApi.Infrastructure.Data;
using RealStateApi.Domain.Entities;
using RealStateApi.Application.Common.Helpers;

namespace RealStateApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IOwnerRepository _ownerRepository;
        private readonly IJwtService _jwtService;

        public AuthController(IOwnerRepository ownerRepository, IJwtService jwtService)
        {
            _ownerRepository = ownerRepository;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Authenticates a user using their email and password, returning a JWT on success.
        /// </summary>
        /// <param name="request">The login request containing email and password.</param>
        /// <response code="200">Returns a JWT token for the authenticated user.</response>
        /// <response code="401">Email or password is invalid.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(TokenResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var owner = await _ownerRepository.GetOwnerByEmailAsync(request.Email);
            if (owner == null)
                return Unauthorized(new { Message = "Invalid email or password." });

            if (!VerifyPassword(request.Password, owner.PasswordHash, owner.PasswordSalt))
                return Unauthorized(new { Message = "Invalid email or password." });

            var token = _jwtService.GenerateToken(owner.Id);

            return Ok(new TokenResponse { Token = token });
        }

        /// <summary>
        /// Registers a new user record with a hashed password.
        /// </summary>
        /// <param name="request">The registration request containing email and password.</param>
        /// <response code="200">Registration successful.</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegistrationResponse), 200)]
        public async Task<IActionResult> Register([FromBody] LoginRequest request)
        {
            var (hash, salt) = PasswordHelper.HashPassword(request.Password);

            var newOwner = new Owner
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            await _ownerRepository.CreateOwnerAsync(newOwner);
            return Ok(new RegistrationResponse { Message = "Registration successful" });
        }

        private bool VerifyPassword(string plainPassword, string storedHash, string storedSalt)
        {
            return PasswordHelper.VerifyPassword(plainPassword, storedHash, storedSalt);
        }
    }

    /// <summary>
    /// Represents the login or registration request with email and password.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// The email address used for authentication or registration.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// The user's plain-text password.
        /// </summary>
        public string Password { get; set; } = null!;
    }

    /// <summary>
    /// Contains the JWT token returned on successful authentication.
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// The JWT token string.
        /// </summary>
        public string Token { get; set; } = null!;
    }

    /// <summary>
    /// Represents a success message for a registration request.
    /// </summary>
    public class RegistrationResponse
    {
        /// <summary>
        /// The result message.
        /// </summary>
        public string Message { get; set; } = null!;
    }

    /// <summary>
    /// Represents an error response when the operation is not authorized.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// The error message explaining why the request was unauthorized.
        /// </summary>
        public string Message { get; set; } = null!;
    }
}
