using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MessManagementSystem.Data;
using MessManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MessManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Since we permanently delete teachers, all teachers in DB are active
            var totalTeachers = await _context.Teachers.CountAsync();
            ViewBag.TotalTeachers = totalTeachers;
            ViewBag.ActiveTeachers = totalTeachers;
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TodayAttendance = await _context.Attendances
                .CountAsync(a => a.Date.Date == DateTime.Today);

            var recentAttendance = await _context.Attendances
                .Include(a => a.Teacher)
                .OrderByDescending(a => a.RecordedDate)
                .Take(10)
                .ToListAsync();

            return View(recentAttendance);
        }

        // GET: Admin/ApiDemo - Demonstrates JWT API usage with fetch/AJAX
        public IActionResult ApiDemo()
        {
            return View();
        }

        // GET: Admin/FormValidationDemo - Demonstrates client + server validation
        public IActionResult FormValidationDemo()
        {
            return View(new Teacher 
            { 
                FullName = "", 
                Email = "", 
                PhoneNumber = "", 
                Department = "" 
            });
        }

        // POST: Admin/FormValidationDemo - Server-side validation example
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FormValidationDemo(Teacher teacher, string username, string password)
        {
            // Remove navigation property validation
            ModelState.Remove("User");
            ModelState.Remove("Attendances");
            ModelState.Remove("Bills");

            // Custom server-side validation
            if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            {
                ModelState.AddModelError("username", "Username must be at least 3 characters");
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            {
                ModelState.AddModelError("username", "Username can only contain letters, numbers and underscores");
            }
            else if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                ModelState.AddModelError("username", "Username already exists");
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                ModelState.AddModelError("password", "Password must be at least 6 characters");
            }

            // Check duplicate email (business logic validation)
            if (!string.IsNullOrEmpty(teacher.Email) && await _context.Teachers.AnyAsync(t => t.Email == teacher.Email))
            {
                ModelState.AddModelError("Email", "This email address is already registered");
            }

            ViewBag.Username = username;

            if (!ModelState.IsValid)
            {
                return View(teacher);
            }

            // Success - show message but don't actually save (demo only)
            ViewBag.Success = "All validations passed! In production, data would be saved here.";
            return View(new Teacher { FullName = "", Email = "", PhoneNumber = "", Department = "" });
        }

        // GET: Admin/AjaxDemo - AJAX operations demo (no page refresh)
        public IActionResult AjaxDemo()
        {
            return View();
        }

        // GET: Admin/GetTeachersJson - Returns teachers as JSON for AJAX
        [HttpGet]
        public async Task<IActionResult> GetTeachersJson()
        {
            var teachers = await _context.Teachers
                .Where(t => t.IsActive)
                .Select(t => new
                {
                    t.TeacherId,
                    t.FullName,
                    t.Email,
                    t.PhoneNumber,
                    t.Department,
                    t.JoiningDate
                })
                .OrderBy(t => t.FullName)
                .ToListAsync();
            return Json(teachers);
        }

        // POST: Admin/CreateTeacherAjax - Create teacher via AJAX
        [HttpPost]
        public async Task<IActionResult> CreateTeacherAjax([FromBody] CreateTeacherAjaxModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Email))
                {
                    return Json(new { success = false, message = "Name and Email are required" });
                }

                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                {
                    return Json(new { success = false, message = "Username already exists" });
                }

                if (await _context.Teachers.AnyAsync(t => t.Email == model.Email))
                {
                    return Json(new { success = false, message = "Email already exists" });
                }

                var user = new User
                {
                    Username = model.Username ?? model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password ?? "changeme123"),
                    Role = "Teacher",
                    MustChangePassword = true,
                    IsActive = true
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var teacher = new Teacher
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber ?? "",
                    Department = model.Department ?? "",
                    UserId = user.UserId,
                    JoiningDate = DateTime.Now,
                    IsActive = true
                };
                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Teacher created successfully",
                    teacher = new {
                        teacher.TeacherId,
                        teacher.FullName,
                        teacher.Email,
                        teacher.PhoneNumber,
                        teacher.Department
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Admin/UpdateTeacherAjax - Update teacher via AJAX
        [HttpPost]
        public async Task<IActionResult> UpdateTeacherAjax([FromBody] UpdateTeacherAjaxModel model)
        {
            try
            {
                var teacher = await _context.Teachers.FindAsync(model.TeacherId);
                if (teacher == null)
                {
                    return Json(new { success = false, message = "Teacher not found" });
                }

                teacher.FullName = model.FullName;
                teacher.Email = model.Email;
                teacher.PhoneNumber = model.PhoneNumber;
                teacher.Department = model.Department;

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Teacher updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Admin/DeleteTeacherAjax - Delete teacher via AJAX
        [HttpPost]
        public async Task<IActionResult> DeleteTeacherAjax(int id)
        {
            try
            {
                var teacher = await _context.Teachers
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.TeacherId == id);

                if (teacher == null)
                {
                    return Json(new { success = false, message = "Teacher not found" });
                }

                if (teacher.User != null)
                {
                    _context.Users.Remove(teacher.User);
                }
                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Teacher deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Admin/ManageTeachers
        public async Task<IActionResult> ManageTeachers()
        {
            var teachers = await _context.Teachers
                .Include(t => t.User)
                .OrderBy(t => t.FullName)
                .ToListAsync();
            return View(teachers);
        }

        // GET: Admin/CreateTeacher
        public IActionResult CreateTeacher()
        {
            return View();
        }

        // POST: Admin/CreateTeacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeacher(Teacher teacher, string username, string password)
        {
            // Remove validation errors for navigation properties
            ModelState.Remove("User");
            ModelState.Remove("Attendances");
            ModelState.Remove("Bills");

            // Validate required fields manually
            if (string.IsNullOrWhiteSpace(username))
            {
                ViewBag.Error = "Username is required.";
                return View(teacher);
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                ViewBag.Error = "Password must be at least 6 characters.";
                return View(teacher);
            }

            if (ModelState.IsValid)
            {
                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username == username))
                {
                    ViewBag.Error = "Username already exists. Please choose a different username.";
                    return View(teacher);
                }

                // Check if email already exists
                if (await _context.Teachers.AnyAsync(t => t.Email == teacher.Email))
                {
                    ViewBag.Error = "Email already exists. Please use a different email.";
                    return View(teacher);
                }

                // Create user account for teacher
                var user = new User
                {
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Role = "Teacher",
                    MustChangePassword = true,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Link teacher to user
                teacher.UserId = user.UserId;
                teacher.JoiningDate = DateTime.Now;
                teacher.IsActive = true;

                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Teacher '{teacher.FullName}' has been successfully added!";
                return RedirectToAction(nameof(ManageTeachers));
            }

            // Show validation errors
            ViewBag.Error = "Please fix the validation errors and try again.";
            return View(teacher);
        }

        // GET: Admin/EditTeacher/5
        public async Task<IActionResult> EditTeacher(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TeacherId == id);

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // POST: Admin/EditTeacher/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeacher(int id, Teacher teacher)
        {
            if (id != teacher.TeacherId)
            {
                return NotFound();
            }

            // Remove validation errors for navigation properties
            ModelState.Remove("User");
            ModelState.Remove("Attendances");
            ModelState.Remove("Bills");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Teacher '{teacher.FullName}' has been successfully updated!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TeacherExists(teacher.TeacherId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ManageTeachers));
            }
            return View(teacher);
        }

        // POST: Admin/DeleteTeacher/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.Attendances)
                .Include(t => t.Bills)
                .FirstOrDefaultAsync(t => t.TeacherId == id);

            if (teacher != null)
            {
                string teacherName = teacher.FullName;

                // Delete all related attendance records
                if (teacher.Attendances.Any())
                {
                    _context.Attendances.RemoveRange(teacher.Attendances);
                }

                // Delete all related bills
                if (teacher.Bills.Any())
                {
                    _context.Bills.RemoveRange(teacher.Bills);
                }

                // Delete the associated user account
                if (teacher.UserId != null && teacher.User != null)
                {
                    _context.Users.Remove(teacher.User);
                }

                // Delete the teacher record
                _context.Teachers.Remove(teacher);

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Teacher '{teacherName}' and all associated data have been permanently deleted!";
            }

            return RedirectToAction(nameof(ManageTeachers));
        }

        // GET: Admin/ViewTeacherDetails/5
        public async Task<IActionResult> ViewTeacherDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.Attendances.OrderByDescending(a => a.Date).Take(30))
                .Include(t => t.Bills.OrderByDescending(b => b.Year).ThenByDescending(b => b.Month).Take(6))
                .FirstOrDefaultAsync(t => t.TeacherId == id);

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        private async Task<bool> TeacherExists(int id)
        {
            return await _context.Teachers.AnyAsync(e => e.TeacherId == id);
        }

        // GET: Admin/ManageDisputes
        public async Task<IActionResult> ManageDisputes(string status = "Pending")
        {
            ViewBag.CurrentStatus = status;
            
            var disputes = await _context.AttendanceDisputes
                .Include(d => d.Attendance)
                .Include(d => d.Teacher)
                .Include(d => d.ResolvedByUser)
                .Where(d => d.Status == status)
                .OrderByDescending(d => d.ReportedDate)
                .ToListAsync();

            return View(disputes);
        }

        // GET: Admin/ViewDispute/5
        public async Task<IActionResult> ViewDispute(int id)
        {
            var dispute = await _context.AttendanceDisputes
                .Include(d => d.Attendance)
                .Include(d => d.Teacher)
                .Include(d => d.ResolvedByUser)
                .FirstOrDefaultAsync(d => d.DisputeId == id);

            if (dispute == null)
            {
                return NotFound();
            }

            // Get billing configuration for calculation
            var config = await _context.BillingConfigurations.FirstOrDefaultAsync();
            ViewBag.BillingConfig = config;

            return View(dispute);
        }

        // POST: Admin/ResolveDispute
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveDispute(int disputeId, string action, string? adminNotes)
        {
            var dispute = await _context.AttendanceDisputes
                .Include(d => d.Attendance)
                .Include(d => d.Teacher)
                .FirstOrDefaultAsync(d => d.DisputeId == disputeId);

            if (dispute == null)
            {
                TempData["Error"] = "Dispute not found.";
                return RedirectToAction(nameof(ManageDisputes));
            }

            if (dispute.Status != "Pending")
            {
                TempData["Error"] = "This dispute has already been resolved.";
                return RedirectToAction(nameof(ManageDisputes));
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (action == "Approve")
            {
                // Remove the attendance record
                var attendance = dispute.Attendance;
                if (attendance != null)
                {
                    // Recalculate bill for the teacher
                    var teacherId = attendance.TeacherId;
                    var month = attendance.Date.Month;
                    var year = attendance.Date.Year;

                    // Get the bill for that month
                    var bill = await _context.Bills
                        .FirstOrDefaultAsync(b => b.TeacherId == teacherId && b.Month == month && b.Year == year);

                    if (bill != null && !bill.IsPaid)
                    {
                        // Get billing configuration
                        var config = await _context.BillingConfigurations.FirstOrDefaultAsync();
                        if (config != null)
                        {
                            // Calculate deductions
                            decimal deduction = 0;
                            int mealsReduced = 0;

                            if (attendance.BreakfastTaken)
                            {
                                deduction += config.DefaultBreakfastRate;
                                mealsReduced++;
                            }
                            if (attendance.LunchTaken)
                            {
                                deduction += config.DefaultLunchRate;
                                mealsReduced++;
                            }
                            if (attendance.DinnerTaken)
                            {
                                deduction += config.DefaultDinnerRate;
                                mealsReduced++;
                            }

                            // Update bill
                            bill.FoodBill -= deduction;
                            bill.TotalBill -= deduction;
                            bill.UnpaidBalance -= deduction;
                            bill.TotalMealsConsumed -= mealsReduced;
                        }
                    }

                    // Delete the attendance record
                    _context.Attendances.Remove(attendance);
                }

                dispute.Status = "Approved";
                dispute.ResolvedBy = userId;
                dispute.ResolvedDate = DateTime.Now;
                dispute.AdminNotes = adminNotes;

                TempData["Success"] = "Dispute approved. Attendance removed and bill adjusted.";
            }
            else if (action == "Reject")
            {
                dispute.Status = "Rejected";
                dispute.ResolvedBy = userId;
                dispute.ResolvedDate = DateTime.Now;
                dispute.AdminNotes = adminNotes;

                TempData["Success"] = "Dispute rejected. No changes made to attendance or bill.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageDisputes));
        }
    }

    // AJAX Model Classes
    public class CreateTeacherAjaxModel
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class UpdateTeacherAjaxModel
    {
        public int TeacherId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Department { get; set; } = "";
    }
}
