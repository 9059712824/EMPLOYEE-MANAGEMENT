using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMPLOYEE_MANAGEMENT.Models
{
    public class AcademicDetails
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public String Name { get; set; }

        [Required]
        public int StartYear { get; set; }

        [Required]
        public int EndYear { get; set; }

        [Required]
        public string fileName { get; set; }

        [Required]
        public byte[] proof { get; set; }

        [DisplayName("UserId: ")]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public User User { get; set; }

    }
}
