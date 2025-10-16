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
}
