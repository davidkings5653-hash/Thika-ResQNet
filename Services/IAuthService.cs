using ThikaResQNet.DTOs;

namespace ThikaResQNet.Services
{
    public interface IAuthService
    {
        Task<UserDto?> RegisterAsync(string fullName, string phone, string password, string role);
        Task<UserDto?> AuthenticateAsync(string phone, string password);
        Task<string?> GenerateJwtTokenAsync(UserDto user);
    }
}