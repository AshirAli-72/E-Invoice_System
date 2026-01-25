using Microsoft.AspNetCore.Mvc;
using E_Invoice_system.Data;
using E_Invoice_system.Models;
using System.Linq;

namespace E_Invoice_system.Controllers
{
    public class SellerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SellerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Seller Profile";
            var sellers = _context.sellers.ToList();
            return View(sellers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Seller";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Sellers seller)
        {
            if (ModelState.IsValid)
            {
                _context.sellers.Add(seller);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(seller);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var seller = _context.sellers.Find(id);
            if (seller == null) return NotFound();
            ViewData["Title"] = "Edit Seller";
            return View(seller);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Sellers seller)
        {
            if (ModelState.IsValid)
            {
                _context.sellers.Update(seller);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(seller);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var seller = _context.sellers.Find(id);
            if (seller != null)
            {
                _context.sellers.Remove(seller);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
