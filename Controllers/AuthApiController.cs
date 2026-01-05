using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MessManagementSystem.Data;
using MessManagementSystem.Models;
using MessManagementSystem.Services;
using System.Security.Claims;

namespace MessManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public AuthApiController(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { error = "ValidationError", message = "Username and password are required" });
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { error = "AuthenticationError", message = "Invalid username or password" });
            }

            if (!string.IsNullOrEmpty(request.Role) && user.Role != request.Role)
            {
                return Unauthorized(new { error = "AuthenticationError", message = "Invalid credentials for selected role" });
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                token,
                tokenType = "Bearer",
                expiresIn = 28800,
                userId = user.UserId,
                username = user.Username,
                role = user.Role,
                mustChangePassword = user.MustChangePassword
            });
        }

        [HttpPost("change-password")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound(new { error = "NotFound", message = "User not found" });
            }

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            {
                return BadRequest(new { error = "ValidationError", message = "Invalid current password" });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.MustChangePassword = false;
            _context.SaveChanges();

            return Ok(new { message = "Password changed successfully" });
        }

        [HttpGet("validate")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ValidateToken()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = User.FindFirstValue(ClaimTypes.Name);
            var role = User.FindFirstValue(ClaimTypes.Role);

            return Ok(new
            {
                userId,
                username,
                role,
                isValid = true
            });
        }

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetCurrentUser()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId && u.IsActive);

            if (user == null)
            {
                return NotFound(new { error = "NotFound", message = "User not found" });
            }

            return Ok(new
            {
                userId = user.UserId,
                username = user.Username,
                role = user.Role,
                createdDate = user.CreatedDate,
                isActive = user.IsActive
            });
        }
    }

    public class LoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string? Role { get; set; }
    }

    public class ChangePasswordRequest
    {
        public required string OldPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}
