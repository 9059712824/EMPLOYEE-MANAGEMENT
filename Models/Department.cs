using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EMPLOYEE_MANAGEMENT.Models
{
    public class Department
    {
        [Key]
        [DisplayName("Department Id: ")]
        public Guid Id { get; set; }

        [Required]
        [DisplayName("Department Name: ")]
        public String DepartmentName { get; set; }

        [Required]
        [DisplayName("Department Head")]
        public String DepartmentHead { get; set;}
    }
}
