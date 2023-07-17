using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EMPLOYEE_MANAGEMENT.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        [DisplayName("Email: ")]
        [Required]
        public string Email { get; set; }

        [DisplayName("Password: ")]
        [Required]
        public string Password { get; set; }

        [DisplayName("Role: ")]
        [Required]
        public String Role { get; set; }

        [DisplayName("ProfilesetupCompleted")]
        [Required]
        public String ProfilesetupCompleted { get; set; }

        [DisplayName("OTP: ")]
        [Required]
        public double OTP { get; set; }

        [DisplayName("OTPGeneratedTime: ")]
        [Required]
        public DateTime OTPGeneratedTime { get; set; }

        public UserDetails UserDetails { get; set; }

        public AcademicDetails AcademicDetails { get; set; }

        public Experience Experience { get; set; }

    }
}
