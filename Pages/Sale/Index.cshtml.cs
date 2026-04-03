using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using E_Invoice_system.Data;
using E_Invoice_system.Models;
using System.Text.RegularExpressions;

namespace E_Invoice_system.Pages.Sale
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<SaleDisplayItem> Sales { get; set; } = new List<SaleDisplayItem>();
        public IList<ReturnDetail> Returns { get; set; } = default!;

        public class SaleDisplayItem
        {
            public int id { get; set; }
            public string? BillNo { get; set; }
            public DateTime Date { get; set; }
            public string? ProductName { get; set; }
            public string DisplayQty { get; set; } = "";
            public decimal Price { get; set; }
            public decimal TotalPrice { get; set; }
            public string? PaymentMethod { get; set; }
            public string? Status { get; set; }
            public bool IsReturned { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
                return RedirectToPage("/Account/Login");

            // Sequential fetching to avoid DbContext concurrency issues
            var allSales = await _context.sales
                .AsNoTracking()
                .OrderByDescending(s => s.date)
                .Take(100)
                .ToListAsync();

            Returns = await _context.returns
                .AsNoTracking()
                .OrderByDescending(r => r.Date)
                .Take(100)
                .ToListAsync();

            Sales = allSales.Select(s => new SaleDisplayItem
            {
                id = s.id,
                BillNo = s.billNo,
                Date = s.date,
                ProductName = s.prod_name_service,
                DisplayQty = Regex.Replace(s.qty_unit_type ?? "", @"[^0-9.-]", ""),
                Price = s.price,
                TotalPrice = s.total_price,
                PaymentMethod = s.payment_method,
                Status = s.status,
                IsReturned = s.is_returned
            }).Where(s =>
            {
                if (decimal.TryParse(s.DisplayQty, out decimal q))
                    return q > 0;
                return true;
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var sale = await _context.sales.FindAsync(id);
                if (sale != null)
                {
                    _context.sales.Remove(sale);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.sales.Any(s => s.id == id))
                {
                    return RedirectToPage();
                }
                throw;
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteReturnAsync(int id)
        {
            var returnRecord = await _context.returns.FindAsync(id);
            if (returnRecord != null)
            {
                _context.returns.Remove(returnRecord);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
