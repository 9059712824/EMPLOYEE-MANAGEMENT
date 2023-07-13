using System;
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
        public Role Role { get; set; }

        [DisplayName("ProfilesetupCompleted")]
        [Required]
        public ProfileStatus ProfilesetupCompleted { get; set; }

        public UserDetails UserDetails { get; set; }

    }
}
