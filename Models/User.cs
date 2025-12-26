using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace ThikaResQNet.Models
{
    public enum UserRole
    {
        Public,
        Responder,
        Dispatcher,
        HospitalAdmin,
        Admin
    }

    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; }

        [Required]
        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.Public;

        // Stored as base64 salt and hashed password
        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string PasswordSalt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public void SetPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            PasswordSalt = Convert.ToBase64String(salt);
            PasswordHash = HashPassword(password, salt);
        }

        public bool VerifyPassword(string password)
        {
            var salt = Convert.FromBase64String(PasswordSalt);
            var hash = HashPassword(password, salt);
            return hash == PasswordHash;
        }

        private static string HashPassword(string password, byte[] salt)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000,
                numBytesRequested: 256 / 8));
            return hashed;
        }
    }
}