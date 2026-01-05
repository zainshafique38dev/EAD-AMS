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
    public class AttendanceApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendanceApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult GetAttendance([FromQuery] DateTime? date, [FromQuery] int? teacherId)
        {
            var query = _context.Attendances
                .Include(a => a.Teacher)
                .AsQueryable();

            if (date.HasValue)
            {
                query = query.Where(a => a.Date.Date == date.Value.Date);
            }

            if (teacherId.HasValue)
            {
                query = query.Where(a => a.TeacherId == teacherId.Value);
            }

            var attendance = query
                .OrderByDescending(a => a.Date)
                .Select(a => new
                {
                    a.AttendanceId,
                    a.TeacherId,
                    TeacherName = a.Teacher != null ? a.Teacher.FullName : "Unknown",
                    a.Date,
                    a.BreakfastTaken,
                    a.LunchTaken,
                    a.DinnerTaken,
                    a.Remarks,
                    a.RecordedDate
                })
                .ToList();

            return Ok(attendance);
        }

        [HttpGet("my-attendance")]
        [Authorize(Policy = "TeacherPolicy")]
        public IActionResult GetMyAttendance([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var teacher = _context.Teachers
                .FirstOrDefault(t => t.User != null && t.User.UserId == userId && t.IsActive);

            if (teacher == null)
            {
                return NotFound(new { message = "Teacher profile not found" });
            }

            var query = _context.Attendances
                .Where(a => a.TeacherId == teacher.TeacherId);

            if (startDate.HasValue)
            {
                query = query.Where(a => a.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.Date <= endDate.Value.Date);
            }

            var attendance = query
                .OrderByDescending(a => a.Date)
                .Select(a => new
                {
                    a.AttendanceId,
                    a.Date,
                    a.BreakfastTaken,
                    a.LunchTaken,
                    a.DinnerTaken,
                    a.Remarks,
                    a.RecordedDate
                })
                .ToList();

            return Ok(attendance);
        }

        [HttpPost]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult MarkAttendance([FromBody] MarkAttendanceRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            // Check if attendance already exists
            var existingAttendance = _context.Attendances
                .FirstOrDefault(a => a.TeacherId == request.TeacherId && a.Date.Date == request.Date.Date);

            if (existingAttendance != null)
            {
                return BadRequest(new { message = "Attendance already marked for this date" });
            }

            // Verify teacher exists
            var teacher = _context.Teachers.FirstOrDefault(t => t.TeacherId == request.TeacherId && t.IsActive);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }

            var attendance = new Attendance
            {
                TeacherId = request.TeacherId,
                Date = request.Date.Date,
                BreakfastTaken = request.BreakfastTaken,
                LunchTaken = request.LunchTaken,
                DinnerTaken = request.DinnerTaken,
                RecordedBy = userId,
                RecordedDate = DateTime.Now,
                Remarks = request.Remarks
            };

            _context.Attendances.Add(attendance);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetAttendance), new { }, new
            {
                attendance.AttendanceId,
                attendance.TeacherId,
                attendance.Date,
                attendance.BreakfastTaken,
                attendance.LunchTaken,
                attendance.DinnerTaken,
                attendance.Remarks
            });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult UpdateAttendance(int id, [FromBody] UpdateAttendanceRequest request)
        {
            var attendance = _context.Attendances.FirstOrDefault(a => a.AttendanceId == id);

            if (attendance == null)
            {
                return NotFound(new { message = "Attendance record not found" });
            }

            attendance.BreakfastTaken = request.BreakfastTaken;
            attendance.LunchTaken = request.LunchTaken;
            attendance.DinnerTaken = request.DinnerTaken;
            attendance.Remarks = request.Remarks;

            _context.SaveChanges();

            return Ok(new
            {
                attendance.AttendanceId,
                attendance.TeacherId,
                attendance.Date,
                attendance.BreakfastTaken,
                attendance.LunchTaken,
                attendance.DinnerTaken,
                attendance.Remarks
            });
        }
    }

    public class MarkAttendanceRequest
    {
        public int TeacherId { get; set; }
        public DateTime Date { get; set; }
        public bool BreakfastTaken { get; set; }
        public bool LunchTaken { get; set; }
        public bool DinnerTaken { get; set; }
        public string? Remarks { get; set; }
    }

    public class UpdateAttendanceRequest
    {
        public bool BreakfastTaken { get; set; }
        public bool LunchTaken { get; set; }
        public bool DinnerTaken { get; set; }
        public string? Remarks { get; set; }
    }
}
