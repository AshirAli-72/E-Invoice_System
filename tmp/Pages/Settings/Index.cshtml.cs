using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace E_Invoice_system.Pages.Settings
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Account/Login");
            }

            return Page();
        }
    }
}
