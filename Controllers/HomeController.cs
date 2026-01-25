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

        public IActionResult Index()
        {
            var totalInvoices = _context.invoices.Count();
            var totalBuyers = _context.buyers.Count();
            var totalProducts = _context.products_services.Count();
            
          

            ViewBag.TotalInvoices = totalInvoices;
            ViewBag.TotalBuyers = totalBuyers;
            ViewBag.TotalProducts = totalProducts;
          

            // Stats for charts
            var invoiceStatusData = _context.invoices
                .GroupBy(i => i.status ?? "Pending")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.StatusLabels = invoiceStatusData.Select(x => x.Status).ToArray();
            ViewBag.StatusCounts = invoiceStatusData.Select(x => x.Count).ToArray();

          
           
            var recentInvoices = _context.invoices
                .OrderByDescending(i => i.date)
                .Take(5)
                .ToList();

            return View(recentInvoices);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
