using Microsoft.AspNetCore.Mvc;
using E_Invoice_system.Data;
using E_Invoice_system.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace E_Invoice_system.Controllers
{
    public class SaleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SaleController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Sales";
            var sales = _context.sales
                .OrderByDescending(s => s.date)
                .ToList();
            return View(sales);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "New Sale";
            ViewBag.Buyers = _context.buyers.Where(b => b.status == "Active").ToList();
            ViewBag.Products = _context.products_services
                .Where(p => p.status == "Available")
                .ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string buyer_name, string status, string payment_method, string description, List<Sale> items)
        {
            if (items == null || !items.Any())
            {
                ModelState.AddModelError("", "At least one product/service must be added.");
            }

            if (ModelState.IsValid)
            {
                DateTime now = DateTime.Now;
                foreach (var item in items)
                {
                    item.buyer_name = buyer_name;
                    item.status = status;
                    item.payment_method = payment_method;
                    item.description = description;
                    item.date = now;

                    // Parse numeric quantity
                    decimal qty = 0;
                    if (!string.IsNullOrEmpty(item.qty_unit_type))
                    {
                        var qtyPart = item.qty_unit_type.Trim().Split(' ')[0];
                        decimal.TryParse(qtyPart, out qty);
                    }

                    // Calculate total
                    item.total_price = (item.price * qty) - item.discount;

                    _context.sales.Add(item);
                }
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Products = _context.products_services
                .Where(p => p.status == "Available")
                .ToList();
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var sale = _context.sales.Find(id);
            if (sale == null) return NotFound();

            ViewData["Title"] = "Edit Sale";
            ViewBag.Products = _context.products_services.ToList();
            return View(sale);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Sale sale)
        {
            // Parse numeric quantity from string
            decimal qty = 0;
            if (!string.IsNullOrEmpty(sale.qty_unit_type))
            {
                var qtyPart = sale.qty_unit_type.Split(' ')[0];
                decimal.TryParse(qtyPart, out qty);
            }

            // Recalculate total
            sale.total_price = (sale.price * qty) - sale.discount;

            if (ModelState.IsValid)
            {
                _context.sales.Update(sale);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Products = _context.products_services.ToList();
            return View(sale);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var sale = _context.sales.Find(id);
            if (sale != null)
            {
                _context.sales.Remove(sale);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public JsonResult GetProductDetails(int productId)
        {
            var product = _context.products_services.Find(productId);
            if (product == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                price = product.price,
                discount = product.discount,
                tax = product.tax,
                name = product.prod_name_service
            });
        }
    }
}
