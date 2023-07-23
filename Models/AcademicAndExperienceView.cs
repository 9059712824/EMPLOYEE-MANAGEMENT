namespace EMPLOYEE_MANAGEMENT.Models
{
    public class AcademicAndExperienceView
    {

        public Guid? AcademicId { get; set; }
        public Guid? AcademicUserId { get; set; }

        public String? AcademicName { get; set; }

        public int? AcademicStartYear { get; set; }

        public int? AcademicEndYear { get; set; }

        public String? AcademicFilename { get; set; }

        public byte[]? AcademicProof { get; set; }

        public Guid? ExperienceId { get; set; }

        public Guid? ExperienceUserId { get; set; }

        public String? ExperienceCompanyName { get; set; }

        public DateTime? ExperienceStartDate { get; set; }

        public DateTime? ExperienceEndDate { get; set; }

        public String? ExperienceYearsOfWorking { get; set; }

        public String? ExperienceFilename { get; set; }

        public byte[]? ExperienceProof { get; set; }


    }
}
