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
        public IList<ReturnDetail> Returns { get; set; } = new List<ReturnDetail>();

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

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

            IQueryable<E_Invoice_system.Models.Sale> salesQuery = _context.sales.AsNoTracking();

            TotalCount = await salesQuery.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            // Ensure PageNumber is within bounds
            if (PageNumber < 1) PageNumber = 1;
            if (TotalPages > 0 && PageNumber > TotalPages) PageNumber = TotalPages;

            // Fetch Sales with Projection
            var salesList = await salesQuery
                .OrderByDescending(s => s.date)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(s => new SaleDisplayItem
                {
                    id = s.id,
                    BillNo = s.billNo,
                    Date = s.date,
                    ProductName = s.prod_name_service,
                    DisplayQty = s.qty_unit_type ?? "", // We will clean this in memory below if needed, but projecting first is faster
                    Price = s.price,
                    TotalPrice = s.total_price,
                    PaymentMethod = s.payment_method,
                    Status = s.status,
                    IsReturned = s.is_returned
                })
                .ToListAsync();

            // Refine DisplayQty in memory (necessary due to Regex)
            foreach (var s in salesList)
            {
                s.DisplayQty = Regex.Replace(s.DisplayQty, @"[^0-9.-]", "");
            }

            Sales = salesList.Where(s =>
            {
                if (decimal.TryParse(s.DisplayQty, out decimal q))
                    return q > 0;
                return true;
            }).ToList();

            // Fetch Returns (limited to latest 20 for speed, or could also paginate similarly)
            Returns = await _context.returns
                .AsNoTracking()
                .OrderByDescending(r => r.Date)
                .Take(20)
                .ToListAsync();

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
