using System.ComponentModel.DataAnnotations;

namespace MessManagementSystem.Models
{
    public class Teacher
    {
        [Key]
        public int TeacherId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
        [Display(Name = "Full Name")]
        public required string FullName { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public required string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Department must be between 2 and 100 characters")]
        [Display(Name = "Department")]
        public required string Department { get; set; }

        public DateTime JoiningDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Foreign Key
        public int? UserId { get; set; }
        public User? User { get; set; }

        // Navigation properties
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Bill> Bills { get; set; } = new List<Bill>();
    }
}
