using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public UserDetails UserDetails { get; set; }

    }
}
