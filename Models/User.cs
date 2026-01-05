using System.ComponentModel.DataAnnotations;

namespace MessManagementSystem.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers and underscores")]
        [Display(Name = "Username")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public required string PasswordHash { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [StringLength(50)]
        [Display(Name = "Role")]
        public required string Role { get; set; } // Admin, Teacher, AttendanceTaker

        public bool MustChangePassword { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Navigation property
        public Teacher? Teacher { get; set; }
    }
}
