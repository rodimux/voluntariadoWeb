using System.Security.Claims;
using Volun.Core.Enums;

namespace Volun.Web.Security;

public static class UserExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var identifier = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(identifier, out var id) ? id : null;
    }

    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole(RolSistema.Admin.ToString());

    public static bool IsCoordinador(this ClaimsPrincipal user) => user.IsInRole(RolSistema.Coordinador.ToString());
}
