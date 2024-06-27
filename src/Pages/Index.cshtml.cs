using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AntiforgeryInAPIController.Pages;

[AllowAnonymous]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
