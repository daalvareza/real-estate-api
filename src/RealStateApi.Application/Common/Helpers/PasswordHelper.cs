using System.Security.Cryptography;

namespace RealStateApi.Application.Common.Helpers
{
    public static class PasswordHelper
    {
        public static (string Hash, string Salt) HashPassword(string password)
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }

            // Generate the hash using PBKDF2
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256);
            byte[] hashBytes = pbkdf2.GetBytes(32);

            // Convert salt and hash to Base64 for storage
            string salt = Convert.ToBase64String(saltBytes);
            string hash = Convert.ToBase64String(hashBytes);

            return (Hash: hash, Salt: salt);
        }

        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            // Convert stored salt back to bytes
            byte[] saltBytes = Convert.FromBase64String(storedSalt);

            // Generate hash with the same salt
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256);
            byte[] hashBytes = pbkdf2.GetBytes(32);

            // Compare the computed hash with the stored hash
            string computedHash = Convert.ToBase64String(hashBytes);
            return computedHash == storedHash;
        }
    }
}
