using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using E_Invoice_system.Data;
using E_Invoice_system.Models;
using System.Text.Json;

namespace E_Invoice_system.Pages.Invoice
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public invoices Invoice { get; set; } = default!;
        public string SingleItemExpiry { get; set; } = "N/A";
        public bool IsMultiItem { get; set; } = false;
        public List<Dictionary<string, object>> Items { get; set; } = new();
        public decimal Subtotal { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
                return RedirectToPage("/Account/Login");

            var invoice = await _context.invoices.FirstOrDefaultAsync(i => i.id == id);
            if (invoice == null) return NotFound();

            // Handle Multi-Item JSON
            try
            {
                if (!string.IsNullOrEmpty(invoice.prod_name_service) && invoice.prod_name_service.Trim().StartsWith("["))
                {
                    var jsonElements = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(invoice.prod_name_service);
                    if (jsonElements != null && jsonElements.Count > 0)
                    {
                        IsMultiItem = true;
                        foreach (var item in jsonElements)
                        {
                            var newItem = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                            foreach (var kvp in item)
                            {
                                if (kvp.Value.ValueKind == JsonValueKind.Number)
                                    newItem[kvp.Key] = kvp.Value.GetDecimal();
                                else if (kvp.Value.ValueKind == JsonValueKind.String)
                                    newItem[kvp.Key] = kvp.Value.GetString() ?? "";
                                else
                                    newItem[kvp.Key] = kvp.Value.ToString();
                            }
                            
                            // Ensure 'name' key exists to avoid KeyNotFoundException if serialized differently


                            Items.Add(newItem);
                        }
                    }
                }
            }
            catch { IsMultiItem = false; }

            // Fetch latest customer info
            var customer = await _context.customers.FirstOrDefaultAsync(c => c.name == invoice.customer_name);
            if (customer != null)
            {
                invoice.customer_address = customer.address;
                invoice.customer_contact = customer.contact;
            }

            // Fetch latest seller info
            var seller = await _context.sellers.FirstOrDefaultAsync(s => s.name == invoice.seller_name);
            if (seller != null)
            {
                invoice.seller_address = seller.address;
                invoice.seller_contact = seller.contact;
            }

            // Fetch expiry date for single-item invoices
            if (!IsMultiItem && !string.IsNullOrEmpty(invoice.prod_name_service))
            {
                var product = await _context.products_services.FirstOrDefaultAsync(p => p.prod_name_service == invoice.prod_name_service);
                SingleItemExpiry = product?.expiry_date?.ToString("yyyy-MM-dd") ?? "N/A";
            }

            Invoice = invoice;
            Subtotal = invoice.total_price + invoice.discount;
            return Page();
        }
    }
}
