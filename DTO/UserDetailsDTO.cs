using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace EMPLOYEE_MANAGEMENT.DTO
{
    public class UserDetailsDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public double Number { get; set; }

        public string Gender { get; set; }
        public DateTime DOB { get; set; }

        public int Age { get; set; }

        public string Address { get; set; }
        public string Department { get; set; }

        public double Salary { get; set; }
    }
}
