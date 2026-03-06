using System.Diagnostics;
using E_Invoice_system.Models;
using E_Invoice_system.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace E_Invoice_system.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // COUNTS & TOTAL SALES (Sequential Async to avoid DbContext concurrency issues)
            ViewBag.TotalInvoices = await _context.invoices.CountAsync();
            ViewBag.TotalCustomers = await _context.customers.CountAsync();
            ViewBag.TotalProducts = await _context.products_services.CountAsync();
            
            decimal totalSales = await _context.sales
                .Where(s => s.total_price > 0)
                .SumAsync(s => (decimal?)s.total_price) ?? 0;

            ViewBag.totalSales = Math.Max(0, totalSales);

            // Invoice Status Chart
            var invoiceStatusData = await _context.invoices
                .GroupBy(i => i.status ?? "Pending")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.StatusLabels = invoiceStatusData.Select(x => x.Status).ToArray();
            ViewBag.StatusCounts = invoiceStatusData.Select(x => x.Count).ToArray();

            // Recent invoices
            var recentInvoices = await _context.invoices
                .OrderByDescending(i => i.date)
                .Take(4)
                .ToListAsync();

            // ✅ TREND CHART DATA (Last 7 Days) - Optimized to 1 Query
            var startDate = DateTime.Today.AddDays(-6);
            var trendResults = await _context.invoices
                .Where(inv => inv.date >= startDate)
                .GroupBy(inv => inv.date.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var trendLabels = new List<string>();
            var trendData = new List<int>();

            for (int i = 6; i >= 0; i--)
            {
                var targetDate = DateTime.Today.AddDays(-i);
                trendLabels.Add(targetDate.ToString("MMM dd"));

                var countEntry = trendResults.FirstOrDefault(r => r.Date == targetDate);
                trendData.Add(countEntry?.Count ?? 0);
            }

            ViewBag.TrendLabels = trendLabels.ToArray();
            ViewBag.TrendData = trendData.ToArray();

            return View(recentInvoices);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
