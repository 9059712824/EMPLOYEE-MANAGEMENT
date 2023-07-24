namespace EMPLOYEE_MANAGEMENT.DTO
{
    public class EmployeeDetails
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public double Number { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public int Age {  get; set; }
        public string Address { get; set; }
        public string Department { get; set; }
        public Guid DepartmentHead { get; set; }
        public double Salary { get; set; }
    }
}
