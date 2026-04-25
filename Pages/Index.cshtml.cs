using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using E_Invoice_system.Data;
using E_Invoice_system.Models;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

namespace E_Invoice_system.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;
        private readonly IMemoryCache _cache;
        private readonly Services.CurrencyService _currencyService;

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context, IMemoryCache cache, Services.CurrencyService currencyService)
        {
            _logger = logger;
            _context = context;
            _cache = cache;
            _currencyService = currencyService;
        }

        public class DashboardStats
        {
            public int TotalInvoices { get; set; }
            public int TotalCustomers { get; set; }
            public int TotalProducts { get; set; }
            public decimal TotalSales { get; set; }
            public string[] StatusLabels { get; set; } = Array.Empty<string>();
            public int[] StatusCounts { get; set; } = Array.Empty<int>();
            public string[] TrendLabels { get; set; } = Array.Empty<string>();
            public int[] TrendData { get; set; } = Array.Empty<int>();
        }

        public IEnumerable<invoices> RecentInvoices { get; set; } = new List<invoices>();
        public int TotalInvoices { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalSales { get; set; }
        public string[] StatusLabels { get; set; } = Array.Empty<string>();
        public int[] StatusCounts { get; set; } = Array.Empty<int>();
        public string[] TrendLabels { get; set; } = Array.Empty<string>();
        public int[] TrendData { get; set; } = Array.Empty<int>();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Simple session check - MUST BE FIRST to allow redirection even if DB is slow
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            {
                return RedirectToPage("/Account/Login");
            }

            // Prime currency symbol for the dashboard
            await _currencyService.GetSymbolAsync();

            try
            {
                _context.Database.SetCommandTimeout(60); 
                var sw = System.Diagnostics.Stopwatch.StartNew();

                // LIVE: Statistics (Fetched fresh as requested)
                TotalInvoices = await _context.invoices.AsNoTracking().CountAsync();
                TotalCustomers = await _context.customers.AsNoTracking().CountAsync();
                TotalProducts = await _context.products_services.AsNoTracking().CountAsync();
                TotalSales = await _context.sales.AsNoTracking()
                    .Where(s => s.total_price > 0)
                    .SumAsync(s => (decimal?)s.total_price) ?? 0;

                var invoiceStatusData = await _context.invoices
                    .AsNoTracking()
                    .GroupBy(i => i.status ?? "Pending")
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                StatusLabels = invoiceStatusData.Select(x => x.Status).ToArray();
                StatusCounts = invoiceStatusData.Select(x => x.Count).ToArray();

                var startDate = DateTime.Today.AddDays(-6);
                var allInvoices = await _context.invoices.AsNoTracking().ToListAsync();
                var trendResults = allInvoices
                    .Where(inv => {
                        if (DateTime.TryParseExact(inv.date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var dt))
                            return dt >= startDate;
                        return false;
                    })
                    .GroupBy(inv => DateTime.ParseExact(inv.date, "yyyy-MM-dd", null).Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .ToList();

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

                // LIVE: Recent Invoices (Fast query, stay fresh)
                RecentInvoices = await _context.invoices
                    .AsNoTracking()
                    .OrderByDescending(i => i.date)
                    .Take(4)
                    .ToListAsync();

                _logger.LogInformation("Dashboard load completed in {ms}ms", sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard statistics.");
                ErrorMessage = "The database is temporarily busy. Please refresh the page in a few seconds.";
            }

            return Page();
        }
    }
}
