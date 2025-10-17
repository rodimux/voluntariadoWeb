using Volun.Core.Enums;

namespace Volun.Web.Security;

public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string CoordinadorOnly = "CoordinadorOnly";
    public const string VoluntarioOnly = "VoluntarioOnly";
    public const string AdminOrCoordinador = "AdminOrCoordinador";

    public static string RoleName(RolSistema rol) => rol.ToString();
}
