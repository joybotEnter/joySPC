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
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace _1JOY_try.Pages_Reports
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? DepartmentId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        // Off by default: stats reflect only Active employees unless the user opts in.
        [BindProperty(SupportsGet = true)]
        public bool IncludeAllStatuses { get; set; }

        public SelectList DepartmentList { get; set; } = default!;

        public int TotalEmployees { get; set; }
        public decimal AverageSalary { get; set; }
        public decimal TotalPayroll { get; set; }

        public Employee? HighestPaid { get; set; }
        public Employee? LowestPaid { get; set; }
        public Employee? MostRecentHire { get; set; }
        public Employee? LongestTenured { get; set; }

        // Salary range
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public decimal MedianSalary { get; set; }
        public List<Employee> TopFiveHighestPaid { get; set; } = new();

        public List<DepartmentStat> DepartmentStats { get; set; } = new();
        public List<PositionStat> PositionStats { get; set; } = new();
        public List<HiringYearStat> HiringTrend { get; set; } = new();

        // Precomputed so the view never needs an inline @{ } block for this.
        public int MaxHireCount { get; set; } = 1;

        public class DepartmentStat
        {
            public string DepartmentName { get; set; } = "Unassigned";
            public int EmployeeCount { get; set; }
            public decimal AverageSalary { get; set; }
            public decimal PayrollSharePercent { get; set; }
        }

        public class PositionStat
        {
            public string Position { get; set; } = "Unspecified";
            public int EmployeeCount { get; set; }
            public decimal AverageSalary { get; set; }
        }

        public class HiringYearStat
        {
            public int Year { get; set; }
            public int HireCount { get; set; }
        }

        public async Task OnGetAsync()
        {
            DepartmentList = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", DepartmentId);

            var employees = await GetFilteredEmployeesAsync();
            ApplyStats(employees);
        }

        public async Task<IActionResult> OnGetExportPdfAsync()
        {
            var employees = await GetFilteredEmployeesAsync();
            ApplyStats(employees);

            var departmentName = "All Departments";
            if (DepartmentId.HasValue)
            {
                var dept = await _context.Departments.FindAsync(DepartmentId.Value);
                if (dept != null) departmentName = dept.Name;
            }

            var pdfBytes = GeneratePdf(departmentName);

            var fileName = $"EmployeeReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        // Shared filtering logic used by both the page view and the PDF export,
        // so exported PDFs always match whatever filters are on screen.
        private async Task<List<Employee>> GetFilteredEmployeesAsync()
        {
            var query = _context.Employees.Include(e => e.Department).AsQueryable();

            if (!IncludeAllStatuses)
            {
                query = query.Where(e => e.Status == EmployeeStatus.Active);
            }

            if (DepartmentId.HasValue)
            {
                query = query.Where(e => e.DepartmentId == DepartmentId.Value);
            }

            if (StartDate.HasValue)
            {
                query = query.Where(e => e.DateHired >= StartDate.Value);
            }

            if (EndDate.HasValue)
            {
                query = query.Where(e => e.DateHired <= EndDate.Value);
            }

            return await query.ToListAsync();
        }

        private void ApplyStats(List<Employee> employees)
        {
            TotalEmployees = employees.Count;

            if (employees.Any())
            {
                AverageSalary = employees.Average(e => e.Salary);
                TotalPayroll = employees.Sum(e => e.Salary);

                HighestPaid = employees.OrderByDescending(e => e.Salary).First();
                LowestPaid = employees.OrderBy(e => e.Salary).First();
                MostRecentHire = employees.OrderByDescending(e => e.DateHired).First();
                LongestTenured = employees.OrderBy(e => e.DateHired).First();

                MinSalary = employees.Min(e => e.Salary);
                MaxSalary = employees.Max(e => e.Salary);
                MedianSalary = GetMedian(employees.Select(e => e.Salary).ToList());

                TopFiveHighestPaid = employees
                    .OrderByDescending(e => e.Salary)
                    .Take(5)
                    .ToList();
            }
            else
            {
                AverageSalary = 0;
                TotalPayroll = 0;
                HighestPaid = null;
                LowestPaid = null;
                MostRecentHire = null;
                LongestTenured = null;
                MinSalary = 0;
                MaxSalary = 0;
                MedianSalary = 0;
                TopFiveHighestPaid = new List<Employee>();
            }

            DepartmentStats = employees
                .GroupBy(e => e.Department != null ? e.Department.Name : "Unassigned")
                .Select(g => new DepartmentStat
                {
                    DepartmentName = g.Key,
                    EmployeeCount = g.Count(),
                    AverageSalary = g.Average(e => e.Salary),
                    PayrollSharePercent = TotalPayroll > 0
                        ? (g.Sum(e => e.Salary) / TotalPayroll) * 100m
                        : 0
                })
                .OrderByDescending(d => d.EmployeeCount)
                .ToList();

            PositionStats = employees
                .GroupBy(e => string.IsNullOrWhiteSpace(e.Position) ? "Unspecified" : e.Position)
                .Select(g => new PositionStat
                {
                    Position = g.Key,
                    EmployeeCount = g.Count(),
                    AverageSalary = g.Average(e => e.Salary)
                })
                .OrderByDescending(p => p.EmployeeCount)
                .ToList();

            HiringTrend = employees
                .GroupBy(e => e.DateHired.Year)
                .Select(g => new HiringYearStat
                {
                    Year = g.Key,
                    HireCount = g.Count()
                })
                .OrderBy(h => h.Year)
                .ToList();

            MaxHireCount = HiringTrend.Any() ? HiringTrend.Max(h => h.HireCount) : 1;
            if (MaxHireCount <= 0) MaxHireCount = 1;
        }

        private static decimal GetMedian(List<decimal> values)
        {
            if (values.Count == 0) return 0;

            var sorted = values.OrderBy(v => v).ToList();
            var mid = sorted.Count / 2;

            return sorted.Count % 2 == 0
                ? (sorted[mid - 1] + sorted[mid]) / 2m
                : sorted[mid];
        }

        private byte[] GeneratePdf(string departmentName)
        {
            var generatedOn = DateTime.Now;
            var filterRange = StartDate.HasValue || EndDate.HasValue
                ? $"{StartDate?.ToString("MMM d, yyyy") ?? "Any"} - {EndDate?.ToString("MMM d, yyyy") ?? "Any"}"
                : "All time";

            var stats = this;

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Calibri));

                    page.Header().Column(col =>
                    {
                        var statusScope = stats.IncludeAllStatuses ? "All Statuses" : "Active Only";

                        col.Item().Text("Employee Report").FontSize(20).Bold().FontColor("#0b1f3a");
                        col.Item().PaddingTop(4).Text($"Department: {departmentName}   |   Hired: {filterRange}   |   Status: {statusScope}").FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(2).Text($"Generated on {generatedOn:MMM d, yyyy h:mm tt}").FontSize(8).FontColor(Colors.Grey.Medium);
                        col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().PaddingTop(15).Column(col =>
                    {
                        col.Spacing(15);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                            {
                                c.Item().Text("Total Employees").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(stats.TotalEmployees.ToString()).FontSize(16).Bold().FontColor("#0b1f3a");
                            });
                            row.Spacing(10);
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                            {
                                c.Item().Text("Average Salary").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(stats.AverageSalary.ToString("C")).FontSize(16).Bold().FontColor("#0b1f3a");
                            });
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                            {
                                c.Item().Text("Total Payroll").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(stats.TotalPayroll.ToString("C")).FontSize(16).Bold().FontColor("#0b1f3a");
                            });
                        });

                        if (stats.TotalEmployees > 0)
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Highest Paid").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                                    c.Item().Text($"{stats.HighestPaid?.FullName} - {stats.HighestPaid?.Salary.ToString("C")}").FontSize(9);
                                });
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Lowest Paid").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                                    c.Item().Text($"{stats.LowestPaid?.FullName} - {stats.LowestPaid?.Salary.ToString("C")}").FontSize(9);
                                });
                            });

                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Most Recent Hire").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                                    c.Item().Text($"{stats.MostRecentHire?.FullName} - {stats.MostRecentHire?.DateHired:MMM d, yyyy}").FontSize(9);
                                });
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Longest Tenured").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                                    c.Item().Text($"{stats.LongestTenured?.FullName} - {stats.LongestTenured?.DateHired:MMM d, yyyy}").FontSize(9);
                                });
                            });

                            // Salary range
                            col.Item().PaddingTop(5).Text("Salary Range").FontSize(12).Bold().FontColor("#0b1f3a");
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Minimum").FontSize(8).FontColor(Colors.Grey.Darken1);
                                    c.Item().Text(stats.MinSalary.ToString("C")).FontSize(11).Bold();
                                });
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Median").FontSize(8).FontColor(Colors.Grey.Darken1);
                                    c.Item().Text(stats.MedianSalary.ToString("C")).FontSize(11).Bold();
                                });
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Maximum").FontSize(8).FontColor(Colors.Grey.Darken1);
                                    c.Item().Text(stats.MaxSalary.ToString("C")).FontSize(11).Bold();
                                });
                            });

                            // Top 5 highest paid
                            col.Item().PaddingTop(5).Text("Top 5 Highest Paid").FontSize(12).Bold().FontColor("#0b1f3a");
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("#");
                                    header.Cell().Element(HeaderCellStyle).Text("Name");
                                    header.Cell().Element(HeaderCellStyle).Text("Position");
                                    header.Cell().Element(HeaderCellStyle).Text("Salary");

                                    static IContainer HeaderCellStyle(IContainer c) =>
                                        c.DefaultTextStyle(x => x.Bold().FontColor(Colors.White))
                                         .Background("#0b1f3a").Padding(6);
                                });

                                var rank = 1;
                                var isAlt = false;
                                foreach (var emp in stats.TopFiveHighestPaid)
                                {
                                    var bg = isAlt ? Colors.Grey.Lighten4 : Colors.White;
                                    table.Cell().Background(bg).Padding(6).Text(rank.ToString());
                                    table.Cell().Background(bg).Padding(6).Text(emp.FullName);
                                    table.Cell().Background(bg).Padding(6).Text(emp.Position);
                                    table.Cell().Background(bg).Padding(6).Text(emp.Salary.ToString("C"));
                                    rank++;
                                    isAlt = !isAlt;
                                }
                            });

                            // Department breakdown with payroll share
                            col.Item().PaddingTop(5).Text("By Department").FontSize(12).Bold().FontColor("#0b1f3a");
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("Department");
                                    header.Cell().Element(HeaderCellStyle).Text("Employees");
                                    header.Cell().Element(HeaderCellStyle).Text("Avg Salary");
                                    header.Cell().Element(HeaderCellStyle).Text("Payroll Share");

                                    static IContainer HeaderCellStyle(IContainer c) =>
                                        c.DefaultTextStyle(x => x.Bold().FontColor(Colors.White))
                                         .Background("#0b1f3a").Padding(6);
                                });

                                var isAlt = false;
                                foreach (var stat in stats.DepartmentStats)
                                {
                                    var bg = isAlt ? Colors.Grey.Lighten4 : Colors.White;
                                    table.Cell().Background(bg).Padding(6).Text(stat.DepartmentName);
                                    table.Cell().Background(bg).Padding(6).Text(stat.EmployeeCount.ToString());
                                    table.Cell().Background(bg).Padding(6).Text(stat.AverageSalary.ToString("C"));
                                    table.Cell().Background(bg).Padding(6).Text($"{stat.PayrollSharePercent:F1}%");
                                    isAlt = !isAlt;
                                }
                            });

                            // Headcount by position
                            col.Item().PaddingTop(5).Text("By Position").FontSize(12).Bold().FontColor("#0b1f3a");
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("Position");
                                    header.Cell().Element(HeaderCellStyle).Text("Employees");
                                    header.Cell().Element(HeaderCellStyle).Text("Avg Salary");

                                    static IContainer HeaderCellStyle(IContainer c) =>
                                        c.DefaultTextStyle(x => x.Bold().FontColor(Colors.White))
                                         .Background("#0b1f3a").Padding(6);
                                });

                                var isAlt = false;
                                foreach (var stat in stats.PositionStats)
                                {
                                    var bg = isAlt ? Colors.Grey.Lighten4 : Colors.White;
                                    table.Cell().Background(bg).Padding(6).Text(stat.Position);
                                    table.Cell().Background(bg).Padding(6).Text(stat.EmployeeCount.ToString());
                                    table.Cell().Background(bg).Padding(6).Text(stat.AverageSalary.ToString("C"));
                                    isAlt = !isAlt;
                                }
                            });

                            // Hiring trend
                            col.Item().PaddingTop(5).Text("Hiring Trend by Year").FontSize(12).Bold().FontColor("#0b1f3a");
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(6);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("Year");
                                    header.Cell().Element(HeaderCellStyle).Text("Hires");
                                    header.Cell().Element(HeaderCellStyle).Text("");

                                    static IContainer HeaderCellStyle(IContainer c) =>
                                        c.DefaultTextStyle(x => x.Bold().FontColor(Colors.White))
                                         .Background("#0b1f3a").Padding(6);
                                });

                                var isAlt = false;
                                foreach (var trend in stats.HiringTrend)
                                {
                                    var bg = isAlt ? Colors.Grey.Lighten4 : Colors.White;
                                    var barWidth = (float)trend.HireCount / stats.MaxHireCount;

                                    table.Cell().Background(bg).Padding(6).Text(trend.Year.ToString());
                                    table.Cell().Background(bg).Padding(6).Text(trend.HireCount.ToString());
                                    table.Cell().Background(bg).Padding(6).Element(c =>
                                        c.Height(12).Width(barWidth * 150).Background("#0b1f3a"));
                                    isAlt = !isAlt;
                                }
                            });
                        }
                        else
                        {
                            col.Item().Text("No employees match this filter.").FontColor(Colors.Grey.Medium);
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}