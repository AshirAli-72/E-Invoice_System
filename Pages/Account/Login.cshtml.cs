using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using E_Invoice_system.Data;
using E_Invoice_system.Models;

namespace E_Invoice_system.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Email { get; set; } = default!;

        [BindProperty]
        public string Password { get; set; } = default!;

        public IActionResult OnGet()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError(string.Empty, "Email and password are required.");
                return Page();
            }

            try
            {
                var user = await _context.users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.email == Email && u.password == Password);

                if (user != null)
                {
                    HttpContext.Session.SetString("UserEmail", user.email);
                    TempData["Success"] = "Welcome back! Login successful.";
                    return RedirectToPage("/Index");
                }

                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
            }
            
            return Page();
        }
    }
}
