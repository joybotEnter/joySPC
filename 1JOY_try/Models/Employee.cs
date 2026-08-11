using System.ComponentModel.DataAnnotations;

namespace _1JOY_try.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Position { get; set; }

        [Range(0, 1000000)]
        public decimal Salary { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateHired { get; set; }

        public string? PhotoPath { get; set; }

        [Required]
        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

        public int? DepartmentId { get; set; }

        public Department? Department { get; set; }
    }
}