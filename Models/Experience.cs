using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMPLOYEE_MANAGEMENT.Models
{
    public class Experience
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public String CompanyName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        [Required]
        public String YearsOfWorking { get; set; }

        [Required]
        public String fileName { get; set; }

        [Required]
        public byte[] proof { get; set; }

        [DisplayName("UserId: ")]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public User User { get; set; }
    }
}
