using Microsoft.AspNetCore.Mvc;
using E_Invoice_system.Data;
using E_Invoice_system.Models;
using System.Linq;

namespace E_Invoice_system.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Products & Services";
            var products = _context.products_services.ToList();
            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Product/Service";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductService product)
        {
            // Server-side validation for quantity 0
            if (!string.IsNullOrEmpty(product.qty_unit_type))
            {
                var qtyValue = product.qty_unit_type.Trim();
                if (qtyValue == "0" || qtyValue.StartsWith("0 "))
                {
                    product.price = 0;
                    product.discount = 0;
                    product.status = "Out of Stock";
                }
            }

            // Validate discount cannot exceed price
            if (product.discount > product.price)
            {
                ModelState.AddModelError("discount", "Discount cannot be greater than the price.");
                return View(product);
            }

            if (ModelState.IsValid)
            {
                product.date = DateTime.Now;
                _context.products_services.Add(product);
                _context.SaveChanges();
                TempData["Success"] = "Product/Service created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.products_services.Find(id);
            if (product == null) return NotFound();
            ViewData["Title"] = "Edit Product/Service";
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductService product)
        {
            // Server-side validation for quantity 0
            if (!string.IsNullOrEmpty(product.qty_unit_type))
            {
                var qtyValue = product.qty_unit_type.Trim();
                if (qtyValue == "0" || qtyValue.StartsWith("0 "))
                {
                    product.price = 0;
                    product.discount = 0;
                    product.status = "Out of Stock";
                }
            }

            // Validate discount cannot exceed price
            if (product.discount > product.price)
            {
                ModelState.AddModelError("discount", "Discount cannot be greater than the price.");
                return View(product);
            }

            if (ModelState.IsValid)
            {
                _context.products_services.Update(product);
                _context.SaveChanges();
                TempData["Success"] = "Product/Service updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var product = _context.products_services.Find(id);
            if (product != null)
            {
                _context.products_services.Remove(product);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
