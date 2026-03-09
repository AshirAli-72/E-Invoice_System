using Microsoft.AspNetCore.Mvc;
using E_Invoice_system.Data;
using E_Invoice_system.Models;

namespace E_Invoice_system.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Invoices";
            // Removed Include(i => i.items) because 'items' does not exist
            var invoices = _context.invoices
                .OrderByDescending(i => i.date)
                .ToList();
            return View(invoices);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Create Invoice";
            ViewBag.Customers = _context.customers.Where(c => c.status == "Active").ToList();
            ViewBag.Products = _context.products_services.Where(p => p.status == "Available").ToList();
            ViewBag.Sellers = _context.sellers.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(invoices invoice)
        {
            if (ModelState.IsValid)
            {
                if (invoice.date == default)
                {
                    invoice.date = DateTime.Now;
                }

                if (string.IsNullOrEmpty(invoice.invoice_no))
                {
                    invoice.invoice_no = "INV-" + DateTime.Now.ToString("yyyyMMdd") + "-" + (_context.invoices.Count() + 1).ToString("D3");
                }

                var customer = _context.customers.FirstOrDefault(c => c.name == invoice.customer_name);

                if (customer != null)
                {
                    invoice.customer_address = customer.address;
                    invoice.customer_contact = customer.contact;
                }

                _context.invoices.Add(invoice);
                _context.SaveChanges();
                TempData["Success"] = "Invoice generated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Customers = _context.customers.Where(b => b.status == "Active").ToList();
            ViewBag.Products = _context.products_services.Where(p => p.status == "Available").ToList();
            ViewBag.Sellers = _context.sellers.ToList();
            return View(invoice);
        }

        public IActionResult Details(int id)
        {
            var invoice = _context.invoices.FirstOrDefault(i => i.id == id);
            if (invoice == null) return NotFound();

            // Fetch latest customer info
            var customer = _context.customers.FirstOrDefault(c => c.name == invoice.customer_name);
            if (customer != null)
            {
                invoice.customer_address = customer.address;
                invoice.customer_contact = customer.contact;
            }

            // Fetch latest seller info
            var seller = _context.sellers.FirstOrDefault(s => s.name == invoice.seller_name);
            if (seller != null)
            {
                invoice.seller_address = seller.address;
                invoice.seller_contact = seller.contact;
            }

            // Fetch expiry date for single-item invoices
            if (!string.IsNullOrEmpty(invoice.prod_name_service) && !invoice.prod_name_service.Trim().StartsWith("["))
            {
                var product = _context.products_services.FirstOrDefault(p => p.prod_name_service == invoice.prod_name_service);
                ViewBag.SingleItemExpiry = product?.expiry_date?.ToString("yyyy-MM-dd") ?? "N/A";
            }

            ViewData["Title"] = "Invoice #" + (invoice.invoice_no ?? invoice.id.ToString());
            return View(invoice);
        }

        public IActionResult Print(int id)
        {
            var invoice = _context.invoices.FirstOrDefault(i => i.id == id);
            if (invoice == null) return NotFound();

            // Fetch latest customer info
            var customer = _context.customers.FirstOrDefault(c => c.name == invoice.customer_name);
            if (customer != null)
            {
                invoice.customer_address = customer.address;
                invoice.customer_contact = customer.contact;
            }

            // Fetch latest seller info
            var seller = _context.sellers.FirstOrDefault(s => s.name == invoice.seller_name);
            if (seller != null)
            {
                invoice.seller_address = seller.address;
                invoice.seller_contact = seller.contact;
            }

            // Fetch expiry date for single-item invoices
            if (!string.IsNullOrEmpty(invoice.prod_name_service) && !invoice.prod_name_service.Trim().StartsWith("["))
            {
                var product = _context.products_services.FirstOrDefault(p => p.prod_name_service == invoice.prod_name_service);
                ViewBag.SingleItemExpiry = product?.expiry_date?.ToString("yyyy-MM-dd") ?? "N/A";
            }

            return View(invoice);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var invoice = _context.invoices.Find(id);
            if (invoice != null)
            {
                _context.invoices.Remove(invoice);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public JsonResult GetSalesByCustomer(string customerName)
        {
            // Now that customer_name is removed from sales, we can't reliably filter sales by customer 
            // unless we join with invoices or another table. For now, we'll just return an empty list or 
            // handle it differently based on the new schema. Assuming this method is no longer useful for sales filtering.
            
            return Json(new { success = false, message = "Customer tracking removed from individual sales." });

        }
    }
}
