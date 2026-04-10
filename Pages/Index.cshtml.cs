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
            // Prime currency symbol for the dashboard
            await _currencyService.GetSymbolAsync();

            // Simple session check
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                _context.Database.SetCommandTimeout(60); 
                var sw = System.Diagnostics.Stopwatch.StartNew();

                // CACHED: Heavy Statistics (5-minute cache)
                var stats = await _cache.GetOrCreateAsync("DashboardStats", async entry => {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                    _logger.LogInformation("Recalculating Dashboard Statistics...");
                    
                    var newStats = new DashboardStats();
                    newStats.TotalInvoices = await _context.invoices.AsNoTracking().CountAsync();
                    newStats.TotalCustomers = await _context.customers.AsNoTracking().CountAsync();
                    newStats.TotalProducts = await _context.products_services.AsNoTracking().CountAsync();
                    newStats.TotalSales = await _context.sales.AsNoTracking()
                        .Where(s => s.total_price > 0)
                        .SumAsync(s => (decimal?)s.total_price) ?? 0;

                    var invoiceStatusData = await _context.invoices
                        .AsNoTracking()
                        .GroupBy(i => i.status ?? "Pending")
                        .Select(g => new { Status = g.Key, Count = g.Count() })
                        .ToListAsync();

                    newStats.StatusLabels = invoiceStatusData.Select(x => x.Status).ToArray();
                    newStats.StatusCounts = invoiceStatusData.Select(x => x.Count).ToArray();

                    var startDate = DateTime.Today.AddDays(-6);
                    var trendResults = await _context.invoices
                        .AsNoTracking()
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

                    newStats.TrendLabels = trendLabelsList.ToArray();
                    newStats.TrendData = trendDataList.ToArray();
                    return newStats;
                });

                if (stats != null)
                {
                    TotalInvoices = stats.TotalInvoices;
                    TotalCustomers = stats.TotalCustomers;
                    TotalProducts = stats.TotalProducts;
                    TotalSales = stats.TotalSales;
                    StatusLabels = stats.StatusLabels;
                    StatusCounts = stats.StatusCounts;
                    TrendLabels = stats.TrendLabels;
                    TrendData = stats.TrendData;
                }

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
