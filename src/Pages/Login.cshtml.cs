using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AntiforgeryInAPIController.Pages;
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

    [TempData]
    public string? ErrorMessage { get; set; }
    public void OnGet()
    {

    }
}
