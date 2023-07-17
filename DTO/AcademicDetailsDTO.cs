using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EMPLOYEE_MANAGEMENT.DTO
{
    public class AcademicDetailsDTO
    {
        public String Name { get; set; }

        public int StartYear { get; set; }

        public int EndYear { get; set; }
    }
}
