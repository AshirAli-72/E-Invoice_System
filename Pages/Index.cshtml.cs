using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using E_Invoice_system.Data;
using E_Invoice_system.Models;
using System.Diagnostics;

namespace E_Invoice_system.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IEnumerable<invoices> RecentInvoices { get; set; } = default!;
        public int TotalInvoices { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalSales { get; set; }
        public string[] StatusLabels { get; set; } = default!;
        public int[] StatusCounts { get; set; } = default!;
        public string[] TrendLabels { get; set; } = default!;
        public int[] TrendData { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            // Simple session check
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
            {
                return RedirectToPage("/Account/Login");
            }

            TotalInvoices = await _context.invoices.CountAsync();
            TotalCustomers = await _context.customers.CountAsync();
            TotalProducts = await _context.products_services.CountAsync();
            
            decimal sales = await _context.sales
                .Where(s => s.total_price > 0)
                .SumAsync(s => (decimal?)s.total_price) ?? 0;

            TotalSales = Math.Max(0, sales);

            // Invoice Status Chart
            var invoiceStatusData = await _context.invoices
                .GroupBy(i => i.status ?? "Pending")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            StatusLabels = invoiceStatusData.Select(x => x.Status).ToArray();
            StatusCounts = invoiceStatusData.Select(x => x.Count).ToArray();

            // Recent invoices
            RecentInvoices = await _context.invoices
                .OrderByDescending(i => i.date)
                .Take(4)
                .ToListAsync();

            // TREND CHART DATA (Last 7 Days)
            var startDate = DateTime.Today.AddDays(-6);
            var trendResults = await _context.invoices
                .Where(inv => inv.date >= startDate)
                .GroupBy(inv => inv.date.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var trendLabelsList = new List<string>();
            var trendDataList = new List<int>();

            for (int i = 6; i >= 0; i--)
            {
                var targetDate = DateTime.Today.AddDays(-i);
                trendLabelsList.Add(targetDate.ToString("MMM dd"));

                var countEntry = trendResults.FirstOrDefault(r => r.Date == targetDate);
                trendDataList.Add(countEntry?.Count ?? 0);
            }

            TrendLabels = trendLabelsList.ToArray();
            TrendData = trendDataList.ToArray();

            return Page();
        }
    }
}
