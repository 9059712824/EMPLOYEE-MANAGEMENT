using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace EMPLOYEE_MANAGEMENT.DTO
{
    public class AcademicDetailsView
    {
        public Guid Id { get; set; }

        public String Name { get; set; }

        public int StartYear { get; set; }

        public int EndYear { get; set; }

        public string fileName { get; set; }

        public byte[] proof { get; set; }
        public Guid UserId { get; set; }
    }
}
