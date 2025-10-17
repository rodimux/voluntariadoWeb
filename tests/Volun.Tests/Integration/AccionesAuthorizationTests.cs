using System.Net;
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

public class AccionesAuthorizationTests(SecuredWebApplicationFactory factory) : IClassFixture<SecuredWebApplicationFactory>
{
    private readonly SecuredWebApplicationFactory _factory = factory;

    [Fact]
    public async Task Put_ShouldReturnForbidden_WhenCoordinadorNoEsPropietario()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VolunDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var coordinadorPropietario = Guid.NewGuid();
        var accion = Accion.Create(
            "Reforestacion",
            "Plantacion de arboles",
            "Fundacion Verde",
            "Bilbao",
            TipoAccion.Evento,
            "Medio Ambiente",
            DateTimeOffset.UtcNow.AddDays(10),
            DateTimeOffset.UtcNow.AddDays(11),
            50,
            Visibilidad.Publica,
            false,
            coordinadorPropietario,
            GeoLocation.From(43.26, -2.93));

        context.Acciones.Add(accion);
        await context.SaveChangesAsync();

        var otroCoordinador = Guid.NewGuid();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Remove(TestAuthHandler.HeaderName);
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, $"{otroCoordinador};{RolSistema.Coordinador}");

        var request = new UpdateAccionRequest(
            "Reforestacion 2025",
            "Plantacion anual",
            "Bilbao",
            DateTimeOffset.UtcNow.AddDays(12),
            DateTimeOffset.UtcNow.AddDays(13),
            60,
            Visibilidad.Publica,
            "Medio Ambiente",
            "Traer guantes",
            null,
            null);

        var response = await client.PutAsJsonAsync($"/api/v1/acciones/{accion.Id}", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<VolunDbContext>();
        var auditExists = await verificationContext.Auditoria.AnyAsync(a => a.EntidadId == accion.Id);
        Assert.False(auditExists);
    }

    [Fact]
    public async Task Put_ShouldAllowAdmin()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VolunDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var coordinadorPropietario = Guid.NewGuid();
        var accion = Accion.Create(
            "Banco de Alimentos",
            "Clasificación de alimentos",
            "ONG Solidaria",
            "Madrid",
            TipoAccion.Programa,
            "Acción Social",
            DateTimeOffset.UtcNow.AddDays(5),
            DateTimeOffset.UtcNow.AddDays(6),
            80,
            Visibilidad.Publica,
            false,
            coordinadorPropietario,
            GeoLocation.From(40.40, -3.70));

        context.Acciones.Add(accion);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Remove(TestAuthHandler.HeaderName);
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, $"{Guid.NewGuid()};{RolSistema.Admin}");

        var request = new UpdateAccionRequest(
            "Banco de Alimentos 2025",
            "Clasificacion semanal",
            "Madrid",
            DateTimeOffset.UtcNow.AddDays(6),
            DateTimeOffset.UtcNow.AddDays(7),
            100,
            Visibilidad.Publica,
            "Accion Social",
            "Vestimenta comoda",
            null,
            null);

        var response = await client.PutAsJsonAsync($"/api/v1/acciones/{accion.Id}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<VolunDbContext>();
        var audit = await verificationContext.Auditoria
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.EntidadId == accion.Id && a.Accion == "Actualizacion");
        Assert.NotNull(audit);
    }
}
