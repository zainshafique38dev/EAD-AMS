using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MessManagementSystem.Data;
using System.Security.Claims;

namespace MessManagementSystem.Controllers
{
    /// <summary>
    /// Demo API Controller - Demonstrates JWT authentication with role-based authorization
    /// All endpoints require JWT Bearer token and return JSON responses (401/403 on auth failure)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DemoApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DemoApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================
        // ADMIN-ONLY ENDPOINT
        // ============================================
        /// <summary>
        /// GET /api/demo/admin-stats - Admin only access
        /// Returns 401 if no token, 403 if not Admin role
        /// </summary>
        [HttpGet("admin-stats")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult GetAdminStats()
        {
            var stats = new
            {
                TotalTeachers = _context.Teachers.Count(),
                TotalUsers = _context.Users.Count(),
                TodayAttendance = _context.Attendances.Count(a => a.Date.Date == DateTime.Today),
                PendingBills = _context.Bills.Count(b => !b.IsPaid),
                Message = "Admin-only data retrieved successfully",
                AccessedBy = User.FindFirstValue(ClaimTypes.Name),
                Role = User.FindFirstValue(ClaimTypes.Role)
            };

            return Ok(stats);
        }

        // ============================================
        // TEACHER-ONLY ENDPOINT
        // ============================================
        /// <summary>
        /// GET /api/demo/teacher-profile - Teacher only access
        /// Returns 401 if no token, 403 if not Teacher role
        /// </summary>
        [HttpGet("teacher-profile")]
        [Authorize(Policy = "TeacherPolicy")]
        public IActionResult GetTeacherProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            
            var teacher = _context.Teachers.FirstOrDefault(t => t.UserId == userId);
            
            if (teacher == null)
            {
                return NotFound(new { error = "NotFound", message = "Teacher profile not found" });
            }

            var profile = new
            {
                teacher.TeacherId,
                teacher.FullName,
                teacher.Email,
                teacher.PhoneNumber,
                teacher.Department,
                teacher.JoiningDate,
                AttendanceCount = _context.Attendances.Count(a => a.TeacherId == teacher.TeacherId),
                UnpaidBills = _context.Bills.Count(b => b.TeacherId == teacher.TeacherId && !b.IsPaid),
                Message = "Teacher profile retrieved successfully",
                AccessedBy = User.FindFirstValue(ClaimTypes.Name),
                Role = User.FindFirstValue(ClaimTypes.Role)
            };

            return Ok(profile);
        }

        // ============================================
        // SHARED ENDPOINT (Admin OR Teacher)
        // ============================================
        /// <summary>
        /// GET /api/demo/whoami - Both Admin and Teacher can access
        /// </summary>
        [HttpGet("whoami")]
        [Authorize(Policy = "AdminOrTeacherPolicy")]
        public IActionResult WhoAmI()
        {
            return Ok(new
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Username = User.FindFirstValue(ClaimTypes.Name),
                Role = User.FindFirstValue(ClaimTypes.Role),
                Message = "Token is valid",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
