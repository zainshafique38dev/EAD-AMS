using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MessManagementSystem.Data;
using MessManagementSystem.Models;
using MessManagementSystem.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MessManagementSystem.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeachersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeachersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .Include(t => t.Bills)
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
            {
                return NotFound("Teacher profile not found.");
            }

            // Get current month attendance
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var monthlyAttendance = await _context.Attendances
                .Where(a => a.TeacherId == teacher.TeacherId && 
                           a.Date.Month == currentMonth && 
                           a.Date.Year == currentYear)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            ViewBag.MonthlyAttendance = monthlyAttendance;
            ViewBag.TotalMeals = monthlyAttendance.Sum(a => 
                (a.BreakfastTaken ? 1 : 0) + (a.LunchTaken ? 1 : 0) + (a.DinnerTaken ? 1 : 0));

            // Get unpaid bills
            ViewBag.UnpaidBills = await _context.Bills
                .Where(b => b.TeacherId == teacher.TeacherId && !b.IsPaid)
                .OrderBy(b => b.Year).ThenBy(b => b.Month)
                .ToListAsync();

            return View(teacher);
        }

        // GET: Teachers/ViewAttendance
        public async Task<IActionResult> ViewAttendance(int? month, int? year)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
            {
                return NotFound();
            }

            var selectedMonth = month ?? DateTime.Now.Month;
            var selectedYear = year ?? DateTime.Now.Year;

            var attendanceRecords = await _context.Attendances
                .Where(a => a.TeacherId == teacher.TeacherId && 
                           a.Date.Month == selectedMonth && 
                           a.Date.Year == selectedYear)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.TeacherName = teacher.FullName;

            return View(attendanceRecords);
        }

        // GET: Teachers/VerifyAttendance
        public async Task<IActionResult> VerifyAttendance()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
            {
                return NotFound();
            }

            // Get last 30 days attendance
            var startDate = DateTime.Now.AddDays(-30);
            var attendanceRecords = await _context.Attendances
                .Where(a => a.TeacherId == teacher.TeacherId && a.Date >= startDate)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            ViewBag.TeacherName = teacher.FullName;

            return View(attendanceRecords);
        }

        // GET: Teachers/ReportWrongAttendance
        public async Task<IActionResult> ReportWrongAttendance(int? attendanceId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
            {
                return NotFound();
            }

            // Get last 30 days attendance with disputes
            var startDate = DateTime.Now.AddDays(-30);
            var attendances = await _context.Attendances
                .Where(a => a.TeacherId == teacher.TeacherId && a.Date >= startDate)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            ViewBag.TeacherName = teacher.FullName;
            ViewBag.SelectedAttendanceId = attendanceId;

            return View(attendances);
        }

        // POST: Teachers/ReportWrongAttendance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportWrongAttendance(int attendanceId, string reason)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
            {
                return NotFound();
            }

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.AttendanceId == attendanceId && a.TeacherId == teacher.TeacherId);

            if (attendance == null)
            {
                TempData["Error"] = "Attendance record not found.";
                return RedirectToAction(nameof(ReportWrongAttendance));
            }

            // Check if already reported
            var existingDispute = await _context.AttendanceDisputes
                .FirstOrDefaultAsync(d => d.AttendanceId == attendanceId && d.Status == "Pending");

            if (existingDispute != null)
            {
                TempData["Error"] = "You have already reported this attendance. Please wait for admin review.";
                return RedirectToAction(nameof(ReportWrongAttendance));
            }

            // Create new dispute
            var dispute = new AttendanceDispute
            {
                AttendanceId = attendanceId,
                TeacherId = teacher.TeacherId,
                Reason = reason,
                Status = "Pending",
                ReportedDate = DateTime.Now
            };

            _context.AttendanceDisputes.Add(dispute);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Your attendance issue has been reported successfully! Admin will review and respond soon.";
            return RedirectToAction(nameof(ReportWrongAttendance));
        }

        // GET: Teachers/ViewDisputeHistory
        public async Task<IActionResult> ViewDisputeHistory(string? status)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
            {
                return NotFound();
            }

            var disputesQuery = _context.AttendanceDisputes
                .Include(d => d.Attendance)
                .Include(d => d.ResolvedByUser)
                .Where(d => d.TeacherId == teacher.TeacherId);

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                disputesQuery = disputesQuery.Where(d => d.Status == status);
            }

            var disputes = await disputesQuery
                .OrderByDescending(d => d.ReportedDate)
                .ToListAsync();

            ViewBag.SelectedStatus = status ?? "All";
            ViewBag.TeacherName = teacher.FullName;

            return View(disputes);
        }

        // GET: Teachers/ViewBills
        public async Task<IActionResult> ViewBills()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
            {
                return NotFound();
            }

            var bills = await _context.Bills
                .Where(b => b.TeacherId == teacher.TeacherId)
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .ToListAsync();

            ViewBag.TeacherName = teacher.FullName;
            ViewBag.TotalUnpaid = bills.Where(b => !b.IsPaid).Sum(b => b.TotalBill);

            return View(bills);
        }

        // GET: Teachers/MakePayment/5
        public async Task<IActionResult> MakePayment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills
                .FirstOrDefaultAsync(b => b.BillId == id && b.TeacherId == teacher.TeacherId);

            if (bill == null || bill.IsPaid)
            {
                return NotFound();
            }

            // Generate payment token
            bill.PaymentToken = PaymentGatewayService.GeneratePaymentToken();
            await _context.SaveChangesAsync();

            return View(bill);
        }

        // POST: Teachers/ProcessPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int billId, string cardNumber, string cardHolderName, string expiryDate, string cvv)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills
                .FirstOrDefaultAsync(b => b.BillId == billId && b.TeacherId == teacher.TeacherId);

            if (bill == null || bill.IsPaid)
            {
                TempData["Error"] = "Bill not found or already paid.";
                return RedirectToAction(nameof(ViewBills));
            }

            // Process payment through dummy gateway
            var paymentResult = PaymentGatewayService.ProcessPayment(
                cardNumber, 
                cardHolderName, 
                expiryDate, 
                cvv, 
                bill.TotalBill
            );

            if (!paymentResult.IsSuccess)
            {
                TempData["Error"] = paymentResult.ErrorMessage;
                return RedirectToAction(nameof(MakePayment), new { id = billId });
            }

            // Update bill as paid
            bill.IsPaid = true;
            bill.PaidDate = DateTime.Now;
            bill.PaymentMethod = "Credit/Debit Card";
            bill.TransactionId = paymentResult.TransactionId;
            bill.UnpaidBalance = 0; // Clear unpaid balance after successful payment

            await _context.SaveChangesAsync();

            // Clear attendance records for this bill's month after payment
            var startDate = new DateTime(bill.Year, bill.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            var attendancesToClear = await _context.Attendances
                .Where(a => a.TeacherId == bill.TeacherId && 
                           a.Date >= startDate && 
                           a.Date <= endDate)
                .ToListAsync();
            
            if (attendancesToClear.Any())
            {
                _context.Attendances.RemoveRange(attendancesToClear);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"✅ Payment successful! Transaction ID: {bill.TransactionId}. Amount paid: Rs. {bill.TotalBill:N2}. Your attendance records have been cleared.";
            return RedirectToAction(nameof(ViewBills));
        }

        // GET: Teachers/BillDetails/5
        public async Task<IActionResult> BillDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
            {
                return NotFound();
            }

            var bill = await _context.Bills
                .Include(b => b.Teacher)
                .FirstOrDefaultAsync(b => b.BillId == id && b.TeacherId == teacher.TeacherId);

            if (bill == null)
            {
                return NotFound();
            }

            // Get attendance records for the bill's month
            var attendances = await _context.Attendances
                .Where(a => a.TeacherId == teacher.TeacherId 
                    && a.Date.Month == bill.Month 
                    && a.Date.Year == bill.Year)
                .OrderBy(a => a.Date)
                .ToListAsync();

            // Get billing configuration
            var config = await _context.BillingConfigurations.FirstOrDefaultAsync();

            ViewBag.BillingConfig = config;
            ViewBag.Attendances = attendances;

            return View(bill);
        }

        // POST: Teachers/DeleteAttendance/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttendance(int id, string? returnAction = "ViewAttendance")
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
            {
                TempData["Error"] = "Teacher profile not found.";
                return RedirectToAction(returnAction);
            }

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.AttendanceId == id && a.TeacherId == teacher.TeacherId);

            if (attendance == null)
            {
                TempData["Error"] = "Attendance record not found or you don't have permission to delete it.";
                return RedirectToAction(returnAction);
            }

            var attendanceDate = attendance.Date;

            // Check if there's a bill for this attendance
            var bill = await _context.Bills
                .FirstOrDefaultAsync(b => b.TeacherId == teacher.TeacherId 
                    && b.Month == attendance.Date.Month 
                    && b.Year == attendance.Date.Year);

            if (bill != null)
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
                    bill.TotalMealsConsumed -= mealsReduced;

                    if (bill.IsPaid)
                    {
                        // If bill is paid, create a credit for next bill
                        bill.UnpaidBalance = -deduction; // Negative balance = credit
                        TempData["Success"] = $"Attendance deleted for {attendanceDate:MMM dd, yyyy}. Credit of ₹{deduction:N2} will be applied to your next bill.";
                    }
                    else
                    {
                        // If unpaid, just reduce the current bill
                        bill.UnpaidBalance -= deduction;
                        TempData["Success"] = $"Attendance deleted for {attendanceDate:MMM dd, yyyy}. Your bill has been reduced by ₹{deduction:N2}.";
                    }
                }
            }

            // Delete the attendance record
            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();

            if (bill == null)
            {
                TempData["Success"] = $"Attendance deleted for {attendanceDate:MMM dd, yyyy}.";
            }

            return RedirectToAction(returnAction);
        }
    }
}
