using System;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volun.Core.Entities;
using Volun.Core.Enums;
using Volun.Infrastructure.Persistence;
using Xunit;

namespace Volun.Tests.Integration;

[Collection(nameof(IntegrationTestsCollection))]
public class VoluntariosAuthorizationTests(SecuredWebApplicationFactory factory) : IClassFixture<SecuredWebApplicationFactory>
{
    private readonly SecuredWebApplicationFactory _factory = factory;

    [Fact]
    public async Task Delete_ShouldAnonymize_WhenVoluntarioEsPropietario()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VolunDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var voluntario = Voluntario.Create(
            "Laura",
            "Sanchez",
            "laura@example.com",
            DateTimeOffset.UtcNow.AddYears(-25),
            true,
            DateTimeOffset.UtcNow,
            telefono: "600123123",
            direccion: "Calle 1",
            provincia: "Madrid",
            pais: "Espana",
            disponibilidad: "Fines de semana");
        voluntario.EstablecerHabilidades(new[] { "Logistica" });
        voluntario.EstablecerPreferencias(new[] { "Reforestacion" });

        context.Voluntarios.Add(voluntario);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        var actorId = Guid.NewGuid();
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, $"{actorId};{RolSistema.Voluntario};{voluntario.Id}");

        var response = await client.DeleteAsync($"/api/v1/voluntarios/{voluntario.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<VolunDbContext>();
        var refreshed = await verificationContext.Voluntarios.AsNoTracking().FirstAsync(v => v.Id == voluntario.Id);

        Assert.False(refreshed.EstaActivo);
        Assert.StartsWith("anon-", refreshed.Email);
        Assert.Equal(DateTimeOffset.UnixEpoch, refreshed.FechaNacimiento);
        Assert.Empty(refreshed.Preferencias);
        Assert.Empty(refreshed.Habilidades);

        var audit = await verificationContext.Auditoria
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.EntidadId == voluntario.Id && a.Accion == "Anonimizacion");
        Assert.NotNull(audit);
    }

    [Fact]
    public async Task Delete_ShouldReturnForbidden_WhenOtroVoluntario()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VolunDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var voluntario = Voluntario.Create(
            "Carlos",
            "Diaz",
            "carlos@example.com",
            DateTimeOffset.UtcNow.AddYears(-30),
            true,
            DateTimeOffset.UtcNow);
        context.Voluntarios.Add(voluntario);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, $"{Guid.NewGuid()};{RolSistema.Voluntario};{Guid.NewGuid()}");

        var response = await client.DeleteAsync($"/api/v1/voluntarios/{voluntario.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<VolunDbContext>();
        var auditExists = await verificationContext.Auditoria
            .AnyAsync(a => a.EntidadId == voluntario.Id && a.Accion == "Anonimizacion");
        Assert.False(auditExists);
    }

    [Fact]
    public async Task Delete_ShouldAllowAdmin()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VolunDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var voluntario = Voluntario.Create(
            "Elena",
            "Lopez",
            "elena@example.com",
            DateTimeOffset.UtcNow.AddYears(-28),
            true,
            DateTimeOffset.UtcNow);
        context.Voluntarios.Add(voluntario);
        await context.SaveChangesAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, $"{Guid.NewGuid()};{RolSistema.Admin}");

        var response = await client.DeleteAsync($"/api/v1/voluntarios/{voluntario.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<VolunDbContext>();
        var audit = await verificationContext.Auditoria
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.EntidadId == voluntario.Id && a.Accion == "Anonimizacion");
        Assert.NotNull(audit);
    }
}
