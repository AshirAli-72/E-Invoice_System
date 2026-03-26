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

        public IList<Models.Sale> Sales { get; set; } = default!;
        public IList<ReturnDetail> Returns { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
                return RedirectToPage("/Account/Login");

            // Filter out sales where qty is 0 (fully returned)
            var allSales = await _context.sales.OrderByDescending(s => s.date).ToListAsync();
            Sales = allSales.Where(s =>
            {
                var match = Regex.Match(s.qty_unit_type ?? "", @"^([0-9.-]+)");
                if (match.Success && decimal.TryParse(match.Groups[1].Value, out decimal q))
                {
                    return q > 0;
                }
                return true;
            }).ToList();

            Returns = await _context.returns.OrderByDescending(r => r.Date).ToListAsync();
            
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
