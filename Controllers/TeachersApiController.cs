using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MessManagementSystem.Data;
using MessManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MessManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TeachersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TeachersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult GetAllTeachers()
        {
            var teachers = _context.Teachers
                .Include(t => t.User)
                .Where(t => t.IsActive)
                .Select(t => new
                {
                    t.TeacherId,
                    t.FullName,
                    t.Email,
                    t.PhoneNumber,
                    t.Department,
                    t.JoiningDate,
                    Username = t.User != null ? t.User.Username : null
                })
                .ToList();

            return Ok(teachers);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOrTeacherPolicy")]
        public IActionResult GetTeacher(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var teacher = _context.Teachers
                .Include(t => t.User)
                .FirstOrDefault(t => t.TeacherId == id && t.IsActive);

            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }

            // Teachers can only view their own data
            if (userRole == "Teacher" && teacher.User?.UserId != userId)
            {
                return Forbid();
            }

            return Ok(new
            {
                teacher.TeacherId,
                teacher.FullName,
                teacher.Email,
                teacher.PhoneNumber,
                teacher.Department,
                teacher.JoiningDate,
                Username = teacher.User?.Username
            });
        }

        [HttpGet("my-profile")]
        [Authorize(Policy = "TeacherPolicy")]
        public IActionResult GetMyProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var teacher = _context.Teachers
                .Include(t => t.User)
                .FirstOrDefault(t => t.User != null && t.User.UserId == userId && t.IsActive);

            if (teacher == null)
            {
                return NotFound(new { message = "Teacher profile not found" });
            }

            return Ok(new
            {
                teacher.TeacherId,
                teacher.FullName,
                teacher.Email,
                teacher.PhoneNumber,
                teacher.Department,
                teacher.JoiningDate
            });
        }

        [HttpPost]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult CreateTeacher([FromBody] CreateTeacherRequest request)
        {
            // Check if username already exists
            if (_context.Users.Any(u => u.Username == request.Username))
            {
                return BadRequest(new { message = "Username already exists" });
            }

            // Check if email already exists
            if (_context.Teachers.Any(t => t.Email == request.Email && t.IsActive))
            {
                return BadRequest(new { message = "Email already exists" });
            }

            // Create user
            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "Teacher",
                MustChangePassword = true,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // Create teacher
            var teacher = new Teacher
            {
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Department = request.Department,
                JoiningDate = DateTime.Now,
                UserId = user.UserId,
                IsActive = true
            };

            _context.Teachers.Add(teacher);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetTeacher), new { id = teacher.TeacherId }, new
            {
                teacher.TeacherId,
                teacher.FullName,
                teacher.Email,
                teacher.PhoneNumber,
                teacher.Department,
                teacher.JoiningDate,
                Username = user.Username
            });
        }
    }

    public class CreateTeacherRequest
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Department { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
