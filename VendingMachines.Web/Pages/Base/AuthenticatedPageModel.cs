using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VendingMachines.Web.Pages.Base
{
    public class AuthenticatedPageModel : PageModel
    {
        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Authorization/Auth");
        }
    }
}
