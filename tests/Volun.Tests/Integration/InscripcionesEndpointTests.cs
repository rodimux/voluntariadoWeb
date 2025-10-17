using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Volun.Core.Entities;
using Volun.Core.Enums;
using Volun.Core.ValueObjects;
using Volun.Infrastructure.Persistence;
using Volun.Web.Dtos;
using Xunit;

namespace Volun.Tests.Integration;

public class InscripcionesEndpointTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory = factory;

    [Fact]
    public async Task Post_ShouldCreateInscripcion_WhenCupoDisponible()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VolunDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var voluntario = Voluntario.Create(
            "Ana",
            "Paredes",
            "ana@example.com",
            DateTimeOffset.UtcNow.AddYears(-25),
            true,
            DateTimeOffset.UtcNow);

        var accion = Accion.Create(
            "Comedor social",
            "Servicio semanal",
            "Fundacion Solidaria",
            "Sevilla",
            TipoAccion.Programa,
            "Social",
            DateTimeOffset.UtcNow.AddDays(2),
            DateTimeOffset.UtcNow.AddDays(3),
            5,
            Visibilidad.Publica,
            false,
            Guid.NewGuid(),
            GeoLocation.From(37.39, -5.99));

        context.Voluntarios.Add(voluntario);
        context.Acciones.Add(accion);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        var request = new CreateInscripcionRequest(voluntario.Id, accion.Id, null, null);

        var response = await client.PostAsJsonAsync("/api/v1/inscripciones", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<InscripcionResponse>();
        Assert.NotNull(payload);
        Assert.Equal(voluntario.Id, payload!.VoluntarioId);
        Assert.Equal(accion.Id, payload.AccionId);
        Assert.Equal(EstadoInscripcion.Pendiente, payload.Estado);
    }

    [Fact]
    public async Task Post_ShouldReturnConflict_WhenTurnoCupoCompleto()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VolunDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var voluntarioTitular = Voluntario.Create(
            "Luis",
            "Santos",
            "luis@example.com",
            DateTimeOffset.UtcNow.AddYears(-30),
            true,
            DateTimeOffset.UtcNow);

        var voluntarioEspera = Voluntario.Create(
            "Maria",
            "Lopez",
            "maria@example.com",
            DateTimeOffset.UtcNow.AddYears(-28),
            true,
            DateTimeOffset.UtcNow);

        var accion = Accion.Create(
            "Recoleccion",
            "Recoleccion solidaria",
            "Fundacion Solidaria",
            "Madrid",
            TipoAccion.Evento,
            "Social",
            DateTimeOffset.UtcNow.AddDays(5),
            DateTimeOffset.UtcNow.AddDays(5).AddHours(4),
            10,
            Visibilidad.Publica,
            true,
            Guid.NewGuid());

        var turno = accion.AgregarTurno(
            "Turno Manana",
            DateTimeOffset.UtcNow.AddDays(5),
            DateTimeOffset.UtcNow.AddDays(5).AddHours(2),
            1,
            null);

        var inscripcionTitular = Inscripcion.Create(voluntarioTitular.Id, accion.Id, turno.Id);
        inscripcionTitular.CambiarEstado(EstadoInscripcion.Aprobada, "Titular");

        context.Voluntarios.AddRange(voluntarioTitular, voluntarioEspera);
        context.Acciones.Add(accion);
        context.Inscripciones.Add(inscripcionTitular);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        var request = new CreateInscripcionRequest(voluntarioEspera.Id, accion.Id, turno.Id, null);

        var response = await client.PostAsJsonAsync("/api/v1/inscripciones", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<InscripcionResponse>();
        Assert.NotNull(payload);
        Assert.Equal(EstadoInscripcion.ListaEspera, payload!.Estado);
        Assert.Equal(turno.Id, payload.TurnoId);
    }

    [Fact]
    public async Task Post_ShouldReturn422_WhenAccionRequiereTurnoPeroNoSeEnvia()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VolunDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var voluntario = Voluntario.Create(
            "Carlos",
            "Perez",
            "carlos@example.com",
            DateTimeOffset.UtcNow.AddYears(-32),
            true,
            DateTimeOffset.UtcNow);

        var accion = Accion.Create(
            "Distribucion Alimentos",
            "Entrega semanal",
            "Fundacion Solidaria",
            "Barcelona",
            TipoAccion.Programa,
            "Social",
            DateTimeOffset.UtcNow.AddDays(3),
            DateTimeOffset.UtcNow.AddDays(3).AddHours(2),
            4,
            Visibilidad.Publica,
            true,
            Guid.NewGuid());

        accion.AgregarTurno(
            "Turno Unico",
            DateTimeOffset.UtcNow.AddDays(3),
            DateTimeOffset.UtcNow.AddDays(3).AddHours(2),
            4,
            null);

        context.Voluntarios.Add(voluntario);
        context.Acciones.Add(accion);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        var request = new CreateInscripcionRequest(voluntario.Id, accion.Id, null, null);

        var response = await client.PostAsJsonAsync("/api/v1/inscripciones", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
