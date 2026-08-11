using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _1JOY_try.Data;
using _1JOY_try.Models;

namespace _1JOY_try.Pages_Employees
{
    public class CreateModel : PageModel
    {
        private readonly _1JOY_try.Data.AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CreateModel(_1JOY_try.Data.AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public SelectList DepartmentList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            DepartmentList = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");
            Employee = new Employee { Status = EmployeeStatus.Active };
            return Page();
        }

        [BindProperty]
        public Employee Employee { get; set; } = default!;

        [BindProperty]
        public IFormFile? PhotoFile { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                DepartmentList = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name");
                return Page();
            }

            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(PhotoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await PhotoFile.CopyToAsync(stream);
                }

                Employee.PhotoPath = "/uploads/" + uniqueFileName;
            }

            _context.Employees.Add(Employee);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}