using System.ComponentModel.DataAnnotations;

namespace EMPLOYEE_MANAGEMENT.DTO
{
    public class ExperienceDTO
    {
        public String CompanyName { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        public String YearsOfWorking { get; set; }
    }
}
