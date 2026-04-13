using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace E_Invoice_system.Pages.Settings.Roles
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
                return RedirectToPage("/Account/Login");
            return Page();
        }
    }
}
