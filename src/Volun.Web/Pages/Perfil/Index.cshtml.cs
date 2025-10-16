using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Volun.Web.Pages.Perfil;

[Authorize]
public class IndexModel : PageModel
{
    public string? Email { get; private set; }

    public void OnGet()
    {
        Email = User.Identity?.Name ?? "voluntario@ejemplo.com";
    }
}
