using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using _1JOY_try.Models;
using _1JOY_try.Data;

namespace _1JOY_try.Pages.DepartmentPages;

public class DeleteModel : PageModel
{
    private readonly AppDbContext _context;

    public DeleteModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Department Department { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var department = await _context.Departments.FirstOrDefaultAsync(m => m.Id == id);
        if (department is null)
        {
            return NotFound();
        }
        else
        {
            Department = department;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var department = await _context.Departments.FindAsync(id);
        if (department != null)
        {
            Department = department;
            _context.Departments.Remove(Department);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
