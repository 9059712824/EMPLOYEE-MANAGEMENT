using System.ComponentModel.DataAnnotations;

namespace EMPLOYEE_MANAGEMENT.DTO
{
    public class RegisterDTO
    {
        [Required]
        public double OTP { get; set; }

        [Required]
        public String Password { get; set; }

        [Required]
        public String NewPassword { get; set; }

        [Required]
        public String ConfirmNewpassword { get; set; }

    }
}
