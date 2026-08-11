using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using _1JOY_try.Data;
using _1JOY_try.Models;

namespace _1JOY_try.Pages_Employees
{
    public class DetailsModel : PageModel
    {
        private readonly _1JOY_try.Data.AppDbContext _context;

        public DetailsModel(_1JOY_try.Data.AppDbContext context)
        {
            _context = context;
        }

        public Employee Employee { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.Include(e => e.Department).FirstOrDefaultAsync(m => m.Id == id);
            if (employee is not null)
            {
                Employee = employee;

                return Page();
            }

            return NotFound();
        }
    }
}
