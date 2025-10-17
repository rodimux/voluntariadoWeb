using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volun.Core.Entities;
using Volun.Core.Enums;
using Volun.Core.ValueObjects;
using Volun.Infrastructure.Persistence;
using Xunit;

namespace Volun.Tests.Integration;

[Collection(nameof(IntegrationTestsCollection))]
public class ExportsAuthorizationTests(SecuredWebApplicationFactory factory) : IClassFixture<SecuredWebApplicationFactory>
{
    private readonly SecuredWebApplicationFactory _factory = factory;

    [Fact]
    public async Task VoluntariosExport_ShouldWriteAudit()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VolunDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var voluntario = Voluntario.Create(
            "Ana",
            "Gomez",
            "ana@example.com",
            DateTimeOffset.UtcNow.AddYears(-25),
            true,
            DateTimeOffset.UtcNow);
        context.Voluntarios.Add(voluntario);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, $"{Guid.NewGuid()};{RolSistema.Admin}");

        var response = await client.GetAsync("/api/v1/export/voluntarios");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<VolunDbContext>();
        var audit = await verificationContext.Auditoria.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Entidad == "ExportVoluntarios" && a.Accion == "ExportCsv");
        Assert.NotNull(audit);
    }

    [Fact]
    public async Task InscripcionesExport_ShouldWriteAudit()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VolunDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var voluntario = Voluntario.Create(
            "Luis",
            "Perez",
            "luis@example.com",
            DateTimeOffset.UtcNow.AddYears(-30),
            true,
            DateTimeOffset.UtcNow);
        var accion = Accion.Create(
            "Banco Alimentos",
            "Clasificacion",
            "Fundacion",
            "Madrid",
            TipoAccion.Evento,
            "Social",
            DateTimeOffset.UtcNow.AddDays(-10),
            DateTimeOffset.UtcNow.AddDays(-9),
            50,
            Visibilidad.Publica,
            false,
            Guid.NewGuid(),
            GeoLocation.From(40.4, -3.7));
        var inscripcion = Inscripcion.Create(voluntario.Id, accion.Id, null);
        context.Voluntarios.Add(voluntario);
        context.Acciones.Add(accion);
        context.Inscripciones.Add(inscripcion);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, $"{Guid.NewGuid()};{RolSistema.Admin}");

        var response = await client.GetAsync("/api/v1/export/inscripciones");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<VolunDbContext>();
        var audit = await verificationContext.Auditoria.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Entidad == "ExportInscripciones" && a.Accion == "ExportCsv");
        Assert.NotNull(audit);
    }

    [Fact]
    public async Task AsistenciasExport_ShouldWriteAudit()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VolunDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var voluntario = Voluntario.Create(
            "Marta",
            "Lopez",
            "marta@example.com",
            DateTimeOffset.UtcNow.AddYears(-28),
            true,
            DateTimeOffset.UtcNow);
        var accion = Accion.Create(
            "Reforestacion",
            "Plantacion",
            "ONG Verde",
            "Bilbao",
            TipoAccion.Evento,
            "Ambiental",
            DateTimeOffset.UtcNow.AddDays(-5),
            DateTimeOffset.UtcNow.AddDays(-4),
            100,
            Visibilidad.Publica,
            false,
            Guid.NewGuid(),
            GeoLocation.From(43.26, -2.93));
        var inscripcion = Inscripcion.Create(voluntario.Id, accion.Id, null);
        var asistencia = Asistencia.Create(inscripcion.Id, DateTimeOffset.UtcNow.AddDays(-1), MetodoAsistencia.Manual, "Presencial");
        asistencia.RegistrarCheckOut(DateTimeOffset.UtcNow, "Completo");
        context.Voluntarios.Add(voluntario);
        context.Acciones.Add(accion);
        context.Inscripciones.Add(inscripcion);
        context.Asistencias.Add(asistencia);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, $"{Guid.NewGuid()};{RolSistema.Admin}");

        var response = await client.GetAsync("/api/v1/export/asistencias");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<VolunDbContext>();
        var audit = await verificationContext.Auditoria.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Entidad == "ExportAsistencias" && a.Accion == "ExportCsv");
        Assert.NotNull(audit);
    }
}
