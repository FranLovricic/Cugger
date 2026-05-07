using System.Security.Cryptography;
using System.Text;

namespace Cugger.Services
{
    /// <summary>
    /// PBKDF2-based password hashing service. Uses SHA-256, 100k iterations, 32B salt + 32B hash.
    /// </summary>
    public class PasswordService
    {
        private const int SaltSize = 32;        // 32 bytes -> 256 bit
        private const int HashSize = 32;        // 32 bytes -> 256 bit
        private const int Iterations = 100_000; // PBKDF2 iterations
        private static readonly HashAlgorithmName Algo = HashAlgorithmName.SHA256;

        public (string hash, string salt) HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));

            var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
            var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
                password: password,
                salt: saltBytes,
                iterations: Iterations,
                hashAlgorithm: Algo,
                outputLength: HashSize);

            return (
                hash: Convert.ToBase64String(hashBytes),
                salt: Convert.ToBase64String(saltBytes)
            );
        }

        public bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(storedSalt))
                return false;

            byte[] saltBytes;
            byte[] storedHashBytes;
            try
            {
                saltBytes = Convert.FromBase64String(storedSalt);
                storedHashBytes = Convert.FromBase64String(storedHash);
            }
            catch (FormatException)
            {
                return false;
            }

            var computed = Rfc2898DeriveBytes.Pbkdf2(
                password: password,
                salt: saltBytes,
                iterations: Iterations,
                hashAlgorithm: Algo,
                outputLength: HashSize);

            return CryptographicOperations.FixedTimeEquals(computed, storedHashBytes);
        }

        public string GenerateResetToken()
        {
            // 64 chars URL-safe token
            var bytes = RandomNumberGenerator.GetBytes(48);
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
    }
}
