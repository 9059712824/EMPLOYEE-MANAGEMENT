using EMPLOYEE_MANAGEMENT.Models;

namespace EMPLOYEE_MANAGEMENT.DTO
{
    public class AcademicAndExperience
    {
        public List<AcademicDetails>? academicDetails { get; set; }

        public List<Experience>? experiences { get; set; }
    }
}
