using Microsoft.AspNetCore.Mvc;
using E_Invoice_system.Data;
using E_Invoice_system.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace E_Invoice_system.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View();
            }

            try
            {
                var user = await _context.users
                    .FirstOrDefaultAsync(u => u.email == email && u.password == password);

                if (user != null)
                {
                    // TEMP session (simple auth)
                    HttpContext.Session.SetString("UserEmail", user.email);

                    TempData["Success"] = "Welcome back! Login successful.";
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Invalid email or password.");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred. Please try again.");
            }
            
            return View();
        }

        public IActionResult Logout()
        {
            return RedirectToAction("Login");
        }
    }
}
