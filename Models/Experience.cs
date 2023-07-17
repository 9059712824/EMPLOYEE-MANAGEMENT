using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMPLOYEE_MANAGEMENT.Models
{
    public class Experience
    {
        [Required]
        public String CompanyName;

        [Required]
        public DateTime StartDate;

        [Required]
        public DateTime EndDate;

        [Required]
        public double YearsOfWorking;

        [Required]
        public byte[] proof { get; set; }

        [DisplayName("UserId: ")]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public User User { get; set; }
    }
}
