using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ThikaResQNet.DTOs;
using ThikaResQNet.Services;

namespace ThikaResQNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await _auth.RegisterAsync(req.FullName, req.PhoneNumber, req.Password, req.Role);
            if (user == null) return BadRequest(new { message = "User exists or invalid role" });
            // Optionally return token on registration
            var token = await _auth.GenerateJwtTokenAsync(user);
            return Ok(new { user, token });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await _auth.AuthenticateAsync(req.PhoneNumber, req.Password);
            if (user == null) return Unauthorized();
            var token = await _auth.GenerateJwtTokenAsync(user);
            return Ok(new { user, token });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(ClaimTypes.Sid) ?? User.FindFirst(ClaimTypes.Name);
            var nameClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity?.Name;
            var phoneClaim = User.FindFirst(ClaimTypes.MobilePhone)?.Value ?? User.FindFirst(ClaimTypes.Upn)?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            var dto = new UserDto
            {
                UserId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : 0,
                FullName = nameClaim ?? string.Empty,
                PhoneNumber = phoneClaim ?? string.Empty,
                Role = Enum.TryParse<ThikaResQNet.Models.UserRole>(roleClaim, true, out var r) ? r : ThikaResQNet.Models.UserRole.Public,
                CreatedAt = DateTime.UtcNow
            };

            return Ok(dto);
        }
    }

    public class RegisterRequest
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } = "Public";
    }

    public class LoginRequest
    {
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
    }
}