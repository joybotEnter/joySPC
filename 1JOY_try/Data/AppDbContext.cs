using Microsoft.EntityFrameworkCore;
using _1JOY_try.Models;

namespace _1JOY_try.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "Engineering" },
                new Department { Id = 2, Name = "Finance" },
                new Department { Id = 3, Name = "IT" },
                new Department { Id = 4, Name = "Human Resources" },
                new Department { Id = 5, Name = "Sales" },
                new Department { Id = 6, Name = "Marketing" }
            );
        }
    }
}