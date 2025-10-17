using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Volun.Core.Entities;
using Volun.Core.Enums;
using Volun.Core.ValueObjects;
using Volun.Web.Security;
using Xunit;

namespace Volun.Tests.Security;

public class CoordinadorOwnsAccionHandlerTests
{
    private static Accion CreateAccion(Guid coordinadorId)
        => Accion.Create(
            "Accion Test",
            "Descripcion",
            "Organizador",
            "Ubicacion",
            TipoAccion.Evento,
            "Categoria",
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(2),
            10,
            Visibilidad.Publica,
            false,
            coordinadorId,
            GeoLocation.From(0, 0));

    [Fact]
    public async Task ShouldSucceed_ForAdminUser()
    {
        var requirement = new CoordinadorOwnsAccionRequirement();
        var handler = new CoordinadorOwnsAccionHandler();
        var accion = CreateAccion(Guid.NewGuid());

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, RolSistema.Admin.ToString())
        }, "Test"));

        var context = new AuthorizationHandlerContext(new[] { requirement }, user, accion);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task ShouldSucceed_ForCoordinadorOwner()
    {
        var requirement = new CoordinadorOwnsAccionRequirement();
        var handler = new CoordinadorOwnsAccionHandler();
        var coordinadorId = Guid.NewGuid();
        var accion = CreateAccion(coordinadorId);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, RolSistema.Coordinador.ToString()),
            new Claim(ClaimTypes.NameIdentifier, coordinadorId.ToString())
        }, "Test"));

        var context = new AuthorizationHandlerContext(new[] { requirement }, user, accion);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task ShouldFail_ForDifferentCoordinador()
    {
        var requirement = new CoordinadorOwnsAccionRequirement();
        var handler = new CoordinadorOwnsAccionHandler();
        var accion = CreateAccion(Guid.NewGuid());

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, RolSistema.Coordinador.ToString()),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        }, "Test"));

        var context = new AuthorizationHandlerContext(new[] { requirement }, user, accion);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }
}
