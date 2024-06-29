using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Antiforgery.Pages;
[AllowAnonymous]
public class LoginModel : PageModel
{
    public class InputModel
    {
        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; } = default!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;
    public string? ReturnUrl { get; set; }

    public async Task OnGet(string? returnUrl = null)
    {
        ReturnUrl = (returnUrl == null || !Url.IsLocalUrl(returnUrl)) ? Url.Content("~/") : returnUrl;

        await HttpContext.SignOutAsync();
    }

    public async Task<IActionResult> OnPost(string? returnUrl = null)
    {
        ReturnUrl = (returnUrl == null || !Url.IsLocalUrl(returnUrl)) ? Url.Content("~/") : returnUrl;

        ClaimsIdentity identity = new (IdentityConstants.ApplicationScheme, ClaimTypes.Name, ClaimTypes.Role);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, Input.UserName));
        identity.AddClaim(new Claim(ClaimTypes.Name, Input.UserName));

        ClaimsPrincipal principal = new (identity);
        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal, new AuthenticationProperties { IsPersistent = true });
        return LocalRedirect(ReturnUrl); 
    }
}
