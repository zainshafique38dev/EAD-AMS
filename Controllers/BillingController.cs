using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MessManagementSystem.Data;
using MessManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MessManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var bills = await _context.Bills
                .Include(b => b.Teacher)
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .Take(50)
                .ToListAsync();

            return View(bills);
        }

        public async Task<IActionResult> Generate(int? month, int? year, int? teacherId)
        {
            var selectedMonth = month ?? DateTime.Now.Month;
            var selectedYear = year ?? DateTime.Now.Year;

            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;

            // Get all active teachers for dropdown
            var teachers = await _context.Teachers
                .Where(t => t.IsActive)
                .OrderBy(t => t.FullName)
                .ToListAsync();
            ViewBag.Teachers = teachers;
            ViewBag.SelectedTeacherId = teacherId;

            // If no POST request (just loading the page), return the view
            if (!Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                return View();
            }

            // Validate teacher selection
            if (teacherId == null || teacherId == 0)
            {
                TempData["Error"] = "Please select a teacher to generate the bill.";
                return View();
            }

            var config = await _context.BillingConfigurations.FirstOrDefaultAsync();

            if (config == null)
            {
                TempData["Error"] = "Billing configuration not found. Please configure billing first.";
                return RedirectToAction(nameof(Configuration));
            }

            // Get selected teacher
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId && t.IsActive);

            if (teacher == null)
            {
                TempData["Error"] = "Teacher not found or inactive.";
                return View();
            }

            var startDate = new DateTime(selectedYear, selectedMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Get attendance for selected teacher only
            var attendances = await _context.Attendances
                .Where(a => a.TeacherId == teacherId && a.Date >= startDate && a.Date <= endDate)
                .ToListAsync();

            var breakfastCount = attendances.Count(a => a.BreakfastTaken);
            var lunchCount = attendances.Count(a => a.LunchTaken);
            var dinnerCount = attendances.Count(a => a.DinnerTaken);

            var foodBill = (breakfastCount * config.DefaultBreakfastRate) +
                           (lunchCount * config.DefaultLunchRate) +
                           (dinnerCount * config.DefaultDinnerRate);

            var totalMeals = breakfastCount + lunchCount + dinnerCount;

            // Calculate water bill per teacher (shared equally)
            var activeTeacherCount = await _context.Teachers.CountAsync(t => t.IsActive);
            var waterBillPerTeacher = activeTeacherCount > 0 ? config.MonthlyWaterBillTotal / activeTeacherCount : 0;

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            // Check if bill already exists
            var existingBill = await _context.Bills
                .FirstOrDefaultAsync(b => b.TeacherId == teacherId && 
                                        b.Month == selectedMonth && 
                                        b.Year == selectedYear);

            // Prevent generating bill if teacher has already paid for this month
            if (existingBill != null && existingBill.IsPaid)
            {
                TempData["Error"] = $"Cannot generate bill! {teacher.FullName} has already paid the bill for {new DateTime(selectedYear, selectedMonth, 1):MMMM yyyy}.";
                return View();
            }

            if (existingBill != null)
            {
                existingBill.FoodBill = foodBill;
                existingBill.WaterBill = waterBillPerTeacher;
                existingBill.TotalBill = foodBill + waterBillPerTeacher + existingBill.UnpaidBalance;
                existingBill.TotalMealsConsumed = totalMeals;
                existingBill.GeneratedDate = DateTime.Now;
                existingBill.GeneratedBy = userId;
                
                await _context.SaveChangesAsync();

                // Clear attendance records after bill generation
                if (attendances.Any())
                {
                    _context.Attendances.RemoveRange(attendances);
                    await _context.SaveChangesAsync();
                }
                
                TempData["Success"] = $"Bill updated successfully for {teacher.FullName} - {new DateTime(selectedYear, selectedMonth, 1):MMMM yyyy}! Attendance records cleared.";
            }
            else
            {
                // Check for previous unpaid balance
                var previousBill = await _context.Bills
                    .Where(b => b.TeacherId == teacherId && !b.IsPaid)
                    .OrderByDescending(b => b.Year)
                    .ThenByDescending(b => b.Month)
                    .FirstOrDefaultAsync();

                var unpaidBalance = previousBill?.UnpaidBalance ?? 0;

                var bill = new Bill
                {
                    TeacherId = teacherId.Value,
                    Teacher = teacher,
                    Month = selectedMonth,
                    Year = selectedYear,
                    FoodBill = foodBill,
                    WaterBill = waterBillPerTeacher,
                    TotalBill = foodBill + waterBillPerTeacher + unpaidBalance,
                    UnpaidBalance = unpaidBalance,
                    TotalMealsConsumed = totalMeals,
                    GeneratedBy = userId,
                    IsPaid = false
                };

                _context.Bills.Add(bill);
                await _context.SaveChangesAsync();

                // Clear attendance records after bill generation
                if (attendances.Any())
                {
                    _context.Attendances.RemoveRange(attendances);
                    await _context.SaveChangesAsync();
                }
                
                TempData["Success"] = $"Bill generated successfully for {teacher.FullName} - {new DateTime(selectedYear, selectedMonth, 1):MMMM yyyy}! Attendance records cleared.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Configuration()
        {
            var config = await _context.BillingConfigurations.FirstOrDefaultAsync();

            if (config == null)
            {
                config = new BillingConfiguration();
            }

            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Configuration(BillingConfiguration config)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                config.UpdatedBy = userId;
                config.LastUpdated = DateTime.Now;

                var existingConfig = await _context.BillingConfigurations.FirstOrDefaultAsync();

                if (existingConfig != null)
                {
                    existingConfig.MonthlyWaterBillTotal = config.MonthlyWaterBillTotal;
                    existingConfig.DefaultBreakfastRate = config.DefaultBreakfastRate;
                    existingConfig.DefaultLunchRate = config.DefaultLunchRate;
                    existingConfig.DefaultDinnerRate = config.DefaultDinnerRate;
                    existingConfig.LastUpdated = config.LastUpdated;
                    existingConfig.UpdatedBy = config.UpdatedBy;
                }
                else
                {
                    _context.BillingConfigurations.Add(config);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Billing configuration updated successfully!";
                return RedirectToAction(nameof(Configuration));
            }

            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill != null)
            {
                bill.IsPaid = true;
                bill.PaidDate = DateTime.Now;
                await _context.SaveChangesAsync();

                // Clear attendance records for this bill's month after marking as paid
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

                TempData["Success"] = "Bill marked as paid! Attendance records cleared.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var bill = await _context.Bills
                .Include(b => b.Teacher)
                .FirstOrDefaultAsync(b => b.BillId == id);

            if (bill == null)
            {
                TempData["Error"] = "Bill not found.";
                return RedirectToAction(nameof(Index));
            }

            // Only allow deletion of paid bills
            if (!bill.IsPaid)
            {
                TempData["Error"] = "Only paid bills can be deleted. Please mark the bill as paid first.";
                return RedirectToAction(nameof(Index));
            }

            var billInfo = $"{bill.Teacher.FullName} - {new DateTime(bill.Year, bill.Month, 1):MMMM yyyy}";

            _context.Bills.Remove(bill);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Bill deleted successfully for {billInfo}!";

            return RedirectToAction(nameof(Index));
        }
    }
}
