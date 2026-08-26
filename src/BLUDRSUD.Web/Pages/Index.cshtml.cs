using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BLUDRSUD.Web.Pages;

[AllowAnonymous]
public class IndexModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Dashboard/Index");
}
