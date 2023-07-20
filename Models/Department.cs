using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [DisplayName("DepartmentHead: ")]
        [ForeignKey("User")]
        public Guid DepartmentHead { get; set; }

        public User User { get; set; }
    }
}
