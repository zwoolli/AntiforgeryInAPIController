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

// TODO: Add a form to test that doesn't need authentication
// And a form to test that does need authentication

// Also add an api endpoint that doesn't need authentication
// And an api endpoint that does need authentication

// Replicate problem!