using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using E_Invoice_system.Data;
using E_Invoice_system.Models;

namespace E_Invoice_system.Pages.Seller
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public IList<Sellers> Sellers { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
                return RedirectToPage("/Account/Login");

            IQueryable<Sellers> query = _context.sellers.AsNoTracking();
            TotalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            
            if (PageNumber < 1) PageNumber = 1;
            if (TotalPages > 0 && PageNumber > TotalPages) PageNumber = TotalPages;

            Sellers = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
                
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var seller = await _context.sellers.FindAsync(id);
            if (seller != null)
            {
                _context.sellers.Remove(seller);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Seller profile deleted.";
            }
            return RedirectToPage();
        }
    }
}
