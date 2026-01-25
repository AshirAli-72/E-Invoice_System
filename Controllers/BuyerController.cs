using Microsoft.AspNetCore.Mvc;
using E_Invoice_system.Data;
using E_Invoice_system.Models;
using System.Linq;

namespace E_Invoice_system.Controllers
{
    public class BuyerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BuyerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Buyers";
            var buyers = _context.buyers.ToList();
            return View(buyers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Buyer";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(buyers buyer)
        {
            if (ModelState.IsValid)
            {
                _context.buyers.Add(buyer);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(buyer);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var buyer = _context.buyers.Find(id);
            if (buyer == null) return NotFound();
            ViewData["Title"] = "Edit Buyer";
            return View(buyer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(buyers buyer)
        {
            if (ModelState.IsValid)
            {
                _context.buyers.Update(buyer);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(buyer);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var buyer = _context.buyers.Find(id);
            if (buyer != null)
            {
                _context.buyers.Remove(buyer);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
