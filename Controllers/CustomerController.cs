using Microsoft.AspNetCore.Mvc;
using E_Invoice_system.Data;
using E_Invoice_system.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace E_Invoice_system.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Customers";
            var customers = _context.customers.ToList();
            return View(customers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Customer";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(customers customer)
        {
            if (ModelState.IsValid)
            {
                _context.customers.Add(customer);
                _context.SaveChanges();
                TempData["Success"] = "Customer created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var customer = _context.customers.Find(id);
            if (customer == null) return NotFound();
            ViewData["Title"] = "Edit Customer";
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(customers customer)
        {
            if (ModelState.IsValid)
            {
                var existingCustomer = _context.customers.AsNoTracking().FirstOrDefault(c => c.id == customer.id);
                if (existingCustomer != null && existingCustomer.name != customer.name)
                {
                    // Update invoices
                    var invoices = _context.invoices.Where(i => i.customer_name == existingCustomer.name).ToList();
                    foreach (var inv in invoices)
                    {
                        inv.customer_name = customer.name;
                    }

                    // Update sales
                    var sales = _context.sales.Where(s => s.customer_name == existingCustomer.name).ToList();
                    foreach (var sale in sales)
                    {
                        sale.customer_name = customer.name;
                    }
                }

                _context.customers.Update(customer);
                _context.SaveChanges();
                TempData["Success"] = "Customer updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var customer = _context.customers.Find(id);
            if (customer != null)
            {
                _context.customers.Remove(customer);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
