using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MessManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace MessManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class MenuApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MenuApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetMenu([FromQuery] string? dayOfWeek, [FromQuery] string? mealType)
        {
            var query = _context.MenuItems
                .Where(m => m.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(dayOfWeek))
            {
                query = query.Where(m => m.DayOfWeek == dayOfWeek);
            }

            if (!string.IsNullOrEmpty(mealType))
            {
                query = query.Where(m => m.MealType == mealType);
            }

            var menu = query
                .OrderBy(m => m.DayOfWeek)
                .ThenBy(m => m.MealType)
                .Select(m => new
                {
                    m.MenuItemId,
                    m.ItemName,
                    m.Description,
                    m.MealType,
                    m.DayOfWeek,
                    m.RatePerServing
                })
                .ToList();

            return Ok(menu);
        }

        [HttpGet("today")]
        [AllowAnonymous]
        public IActionResult GetTodayMenu()
        {
            var today = DateTime.Now.DayOfWeek.ToString();

            var menu = _context.MenuItems
                .Where(m => m.DayOfWeek == today && m.IsActive)
                .OrderBy(m => m.MealType)
                .Select(m => new
                {
                    m.MenuItemId,
                    m.ItemName,
                    m.Description,
                    m.MealType,
                    m.RatePerServing
                })
                .ToList();

            return Ok(new
            {
                day = today,
                menu = menu
            });
        }

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetMenuItem(int id)
        {
            var menuItem = _context.MenuItems
                .FirstOrDefault(m => m.MenuItemId == id && m.IsActive);

            if (menuItem == null)
            {
                return NotFound(new { message = "Menu item not found" });
            }

            return Ok(new
            {
                menuItem.MenuItemId,
                menuItem.ItemName,
                menuItem.Description,
                menuItem.MealType,
                menuItem.DayOfWeek,
                menuItem.RatePerServing
            });
        }

        [HttpPost]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult CreateMenuItem([FromBody] CreateMenuItemRequest request)
        {
            var menuItem = new MessManagementSystem.Models.MenuItem
            {
                ItemName = request.ItemName,
                Description = request.Description,
                MealType = request.MealType,
                DayOfWeek = request.DayOfWeek,
                RatePerServing = request.RatePerServing,
                IsActive = true
            };

            _context.MenuItems.Add(menuItem);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.MenuItemId }, new
            {
                menuItem.MenuItemId,
                menuItem.ItemName,
                menuItem.Description,
                menuItem.MealType,
                menuItem.DayOfWeek,
                menuItem.RatePerServing
            });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult UpdateMenuItem(int id, [FromBody] UpdateMenuItemRequest request)
        {
            var menuItem = _context.MenuItems.FirstOrDefault(m => m.MenuItemId == id);

            if (menuItem == null)
            {
                return NotFound(new { message = "Menu item not found" });
            }

            menuItem.ItemName = request.ItemName;
            menuItem.Description = request.Description;
            menuItem.MealType = request.MealType;
            menuItem.DayOfWeek = request.DayOfWeek;
            menuItem.RatePerServing = request.RatePerServing;

            _context.SaveChanges();

            return Ok(new
            {
                menuItem.MenuItemId,
                menuItem.ItemName,
                menuItem.Description,
                menuItem.MealType,
                menuItem.DayOfWeek,
                menuItem.RatePerServing
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult DeleteMenuItem(int id)
        {
            var menuItem = _context.MenuItems.FirstOrDefault(m => m.MenuItemId == id);

            if (menuItem == null)
            {
                return NotFound(new { message = "Menu item not found" });
            }

            // Soft delete
            menuItem.IsActive = false;
            _context.SaveChanges();

            return Ok(new { message = "Menu item deleted successfully" });
        }
    }

    public class CreateMenuItemRequest
    {
        public required string ItemName { get; set; }
        public required string Description { get; set; }
        public required string MealType { get; set; }
        public required string DayOfWeek { get; set; }
        public decimal RatePerServing { get; set; }
    }

    public class UpdateMenuItemRequest
    {
        public required string ItemName { get; set; }
        public required string Description { get; set; }
        public required string MealType { get; set; }
        public required string DayOfWeek { get; set; }
        public decimal RatePerServing { get; set; }
    }
}
