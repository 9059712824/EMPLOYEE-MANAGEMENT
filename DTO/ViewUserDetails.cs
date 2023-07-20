using System.ComponentModel.DataAnnotations;

namespace EMPLOYEE_MANAGEMENT.DTO
{
    public class ViewUserDetails
    {
        public Guid UserId { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public Double EmployeeNumber { get; set; }
        public String Gender { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DOB { get; set;}
        public int Age { get; set; }
        public String Address { get; set; }
        public String Department { get; set; }

        public Guid DepartmentHead { get; set; }
        public double Salary { get; set; }
    }
}
