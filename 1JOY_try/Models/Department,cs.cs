using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace _1JOY_try.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public ICollection<Employee>? Employees { get; set; }
    }
}