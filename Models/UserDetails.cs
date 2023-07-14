using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMPLOYEE_MANAGEMENT.Models
{
    public class UserDetails
    {
        [DisplayName("FirstName: ")]
        [Required]
        public string FirstName { get; set; }

        [DisplayName("LastName: ")]
        [Required]
        public string LastName { get; set; }

        [DisplayName("EmployeeNumber: ")]
        [Required]
        public double Number { get; set; }

        [DisplayName("Gender: ")]
        [Required]
        public string Gender { get; set; }

        [DisplayName("DOB: ")]
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DOB { get; set; }

        [DisplayName("Age: ")]
        [Required]
        public int Age { get; set; }

        [DisplayName("Address: ")]
        [Required]
        public string Address { get; set; }

        [DisplayName("Department: ")]
        [Required]
        public string Department { get; set; }

        [DisplayName("Salary: ")]
        [Required]
        public double Salary { get; set; }

        [DisplayName("UserId: ")]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public User User { get; set; }
    }
}
