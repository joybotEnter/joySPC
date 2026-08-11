using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _1JOY_try.Data;
using _1JOY_try.Models;

namespace _1JOY_try.Pages_Employees
{
    public class IndexModel : PageModel
    {
        private readonly _1JOY_try.Data.AppDbContext _context;

        private const int PageSize = 6;

        public IndexModel(_1JOY_try.Data.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortOrder { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? DepartmentId { get; set; }

        [BindProperty(SupportsGet = true)]
        public EmployeeStatus? StatusFilter { get; set; }

        public SelectList DepartmentList { get; set; } = default!;

        public int TotalPages { get; set; }

        public IList<Employee> Employee { get; set; } = default!;

        public async Task OnGetAsync()
        {
            if (PageNumber < 1)
            {
                PageNumber = 1;
            }

            DepartmentList = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", DepartmentId);

            var query = _context.Employees.Include(e => e.Department).AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(e => e.FullName.Contains(SearchTerm) || e.Position.Contains(SearchTerm));
            }

            if (DepartmentId.HasValue)
            {
                query = query.Where(e => e.DepartmentId == DepartmentId.Value);
            }

            if (StatusFilter.HasValue)
            {
                query = query.Where(e => e.Status == StatusFilter.Value);
            }

            if (StartDate.HasValue)
            {
                query = query.Where(e => e.DateHired >= StartDate.Value);
            }

            if (EndDate.HasValue)
            {
                query = query.Where(e => e.DateHired <= EndDate.Value);
            }

            query = SortOrder switch
            {
                "name_desc" => query.OrderByDescending(e => e.FullName),
                "position" => query.OrderBy(e => e.Position),
                "position_desc" => query.OrderByDescending(e => e.Position),
                "salary" => query.OrderBy(e => e.Salary),
                "salary_desc" => query.OrderByDescending(e => e.Salary),
                "date" => query.OrderBy(e => e.DateHired),
                "date_desc" => query.OrderByDescending(e => e.DateHired),
                _ => query.OrderBy(e => e.FullName),
            };

            var totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            Employee = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}