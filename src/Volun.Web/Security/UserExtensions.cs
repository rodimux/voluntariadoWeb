using System.Security.Claims;
using Volun.Core.Enums;

namespace Volun.Web.Security;

public static class UserExtensions
{
    public const string VoluntarioIdClaimType = "voluntarioId";
    public const string TestingBypassClaimType = "testing-auth";

    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var identifier = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(identifier, out var id) ? id : null;
    }

    public static Guid? GetVoluntarioId(this ClaimsPrincipal user)
    {
        var identifier = user.FindFirstValue(VoluntarioIdClaimType);
        return Guid.TryParse(identifier, out var id) ? id : null;
    }

    public static bool IsAdmin(this ClaimsPrincipal user) =>
        user.IsInRole(RolSistema.Admin.ToString()) || IsTestingBypass(user);

    public static bool IsCoordinador(this ClaimsPrincipal user) =>
        user.IsInRole(RolSistema.Coordinador.ToString()) || IsTestingBypass(user);

    public static bool IsVoluntario(this ClaimsPrincipal user) =>
        user.IsInRole(RolSistema.Voluntario.ToString()) || IsTestingBypass(user);

    public static bool IsAdminOrCoordinador(this ClaimsPrincipal user)
        => IsAdmin(user) || IsCoordinador(user);

    public static bool CanAccessVoluntario(this ClaimsPrincipal user, Guid voluntarioId)
    {
        var ownVoluntarioId = user.GetVoluntarioId();
        if (ownVoluntarioId.HasValue && ownVoluntarioId.Value == voluntarioId)
        {
            return true;
        }

        return IsAdminOrCoordinador(user);
    }

    private static bool IsTestingBypass(ClaimsPrincipal user) =>
        user.HasClaim(TestingBypassClaimType, "true");
}
