using System.Security.Claims;
using Volun.Core.Enums;
using Volun.Web.Security;
using Xunit;

namespace Volun.Tests.Security;

public class UserExtensionsTests
{
    [Fact]
    public void CanAccessVoluntario_ShouldReturnTrue_ForOwner()
    {
        var voluntarioId = Guid.NewGuid();
        var user = BuildUser(voluntarioId: voluntarioId);

        var result = user.CanAccessVoluntario(voluntarioId);

        Assert.True(result);
    }

    [Fact]
    public void CanAccessVoluntario_ShouldReturnTrue_ForAdmin()
    {
        var voluntarioId = Guid.NewGuid();
        var user = BuildUser(role: RolSistema.Admin);

        var result = user.CanAccessVoluntario(voluntarioId);

        Assert.True(result);
    }

    [Fact]
    public void CanAccessVoluntario_ShouldReturnFalse_ForDifferentVoluntario()
    {
        var voluntarioId = Guid.NewGuid();
        var user = BuildUser(voluntarioId: Guid.NewGuid());

        var result = user.CanAccessVoluntario(voluntarioId);

        Assert.False(result);
    }

    private static ClaimsPrincipal BuildUser(RolSistema? role = null, Guid? voluntarioId = null)
    {
        var claims = new List<Claim>();

        if (role.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Value.ToString()));
        }

        if (voluntarioId.HasValue)
        {
            claims.Add(new Claim(UserExtensions.VoluntarioIdClaimType, voluntarioId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, "Test");
        return new ClaimsPrincipal(identity);
    }
}
