using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ThikaResQNet.DTOs;
using ThikaResQNet.Models;
using ThikaResQNet.Repositories;

namespace ThikaResQNet.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        public async Task<UserDto?> AuthenticateAsync(string phone, string password)
        {
            var user = await _repo.GetByPhoneAsync(phone);
            if (user == null) return null;
            if (!user.VerifyPassword(password)) return null;
            return new UserDto { UserId = user.UserId, FullName = user.FullName, PhoneNumber = user.PhoneNumber, Role = user.Role, CreatedAt = user.CreatedAt };
        }

        public async Task<UserDto?> RegisterAsync(string fullName, string phone, string password, string role)
        {
            var exists = await _repo.GetByPhoneAsync(phone);
            if (exists != null) return null;
            var user = new User { FullName = fullName, PhoneNumber = phone };
            if (Enum.TryParse<UserRole>(role, true, out var parsed)) user.Role = parsed;
            user.SetPassword(password);
            var created = await _repo.AddAsync(user);
            return new UserDto { UserId = created.UserId, FullName = created.FullName, PhoneNumber = created.PhoneNumber, Role = created.Role, CreatedAt = created.CreatedAt };
        }

        public Task<string?> GenerateJwtTokenAsync(UserDto user)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = jwtSection.GetValue<string>("Key");
            var issuer = jwtSection.GetValue<string>("Issuer");
            var audience = jwtSection.GetValue<string>("Audience");
            var expireMinutes = jwtSection.GetValue<int>("ExpireMinutes");

            if (string.IsNullOrEmpty(key)) return Task.FromResult<string?>(null);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.PhoneNumber),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds);

            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
            return Task.FromResult<string?>(tokenStr);
        }
    }
}