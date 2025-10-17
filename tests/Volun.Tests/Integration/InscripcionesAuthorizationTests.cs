using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volun.Core.Entities;
using Volun.Core.Enums;
using Volun.Core.ValueObjects;
using Volun.Infrastructure.Persistence;
using Volun.Web.Dtos;
using Xunit;

namespace Volun.Tests.Integration;

[Collection(nameof(IntegrationTestsCollection))]

public class InscripcionesAuthorizationTests(SecuredWebApplicationFactory factory) : IClassFixture<SecuredWebApplicationFactory>
{
    private readonly SecuredWebApplicationFactory _factory = factory;

    [Fact]
    public async Task PatchEstado_ShouldReturnForbidden_WhenCoordinadorNoEsPropietario()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VolunDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var coordinadorPropietario = Guid.NewGuid();
        var accion = Accion.Create(
            "Campaña Abrigos",
            "Entrega de abrigos",
            "Fundación Calor",
            "Valencia",
            TipoAccion.Campa\u00f1a,
            "Acción Social",
            DateTimeOffset.UtcNow.AddDays(4),
            DateTimeOffset.UtcNow.AddDays(5),
            20,
            Visibilidad.Publica,
            false,
            coordinadorPropietario,
            GeoLocation.From(39.47, -0.38));

        var voluntario = Voluntario.Create(
            "Julia",
            "Martínez",
            "julia@example.com",
            DateTimeOffset.UtcNow.AddYears(-30),
            true,
            DateTimeOffset.UtcNow);

        var inscripcion = Inscripcion.Create(voluntario.Id, accion.Id, null, null);

        context.Acciones.Add(accion);
        context.Voluntarios.Add(voluntario);
        context.Inscripciones.Add(inscripcion);
        await context.SaveChangesAsync();

        var otroCoordinador = Guid.NewGuid();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Remove(TestAuthHandler.HeaderName);
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, $"{otroCoordinador};{RolSistema.Coordinador}");

        var request = new UpdateEstadoInscripcionRequest(EstadoInscripcion.Aprobada, "Aprobado");
        var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/inscripciones/{inscripcion.Id}/estado")
        {
            Content = JsonContent.Create(request)
        };

        var response = await client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<VolunDbContext>();
        var auditExists = await verificationContext.Auditoria
            .AnyAsync(a => a.EntidadId == inscripcion.Id && a.Accion == "CambioEstado");
        Assert.False(auditExists);
    }

    [Fact]
    public async Task PatchEstado_ShouldAllowAdmin()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VolunDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var coordinadorPropietario = Guid.NewGuid();
        var accion = Accion.Create(
            "Recogida Alimentos",
            "Recogida mensual",
            "ONG Solidaria",
            "Madrid",
            TipoAccion.Programa,
            "Acción Social",
            DateTimeOffset.UtcNow.AddDays(7),
            DateTimeOffset.UtcNow.AddDays(7).AddHours(6),
            30,
            Visibilidad.Publica,
            false,
            coordinadorPropietario,
            GeoLocation.From(40.42, -3.70));

        var voluntario = Voluntario.Create(
            "Luis",
            "Ramírez",
            "luis@example.com",
            DateTimeOffset.UtcNow.AddYears(-26),
            true,
            DateTimeOffset.UtcNow);

        var inscripcion = Inscripcion.Create(voluntario.Id, accion.Id, null, null);

        context.Acciones.Add(accion);
        context.Voluntarios.Add(voluntario);
        context.Inscripciones.Add(inscripcion);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Remove(TestAuthHandler.HeaderName);
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, $"{Guid.NewGuid()};{RolSistema.Admin}");

        var request = new UpdateEstadoInscripcionRequest(EstadoInscripcion.Aprobada, "Confirmado");
        var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/inscripciones/{inscripcion.Id}/estado")
        {
            Content = JsonContent.Create(request)
        };

        var response = await client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<VolunDbContext>();
        var audit = await verificationContext.Auditoria
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.EntidadId == inscripcion.Id && a.Accion == "CambioEstado");
        Assert.NotNull(audit);
    }
}
