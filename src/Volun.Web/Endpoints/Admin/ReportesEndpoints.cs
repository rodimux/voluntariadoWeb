using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volun.Infrastructure.Persistence;

namespace Volun.Web.Endpoints.Admin;

public static class ReportesEndpoints
{
    private const string PolicyAdminOrCoordinador = "RequireAdminOrCoordinador";

    public static IEndpointRouteBuilder MapReportesEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/v1/reportes/dashboard", async Task<IResult> (
            VolunDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var voluntariosActivos = await dbContext.Voluntarios.CountAsync(v => v.EstaActivo, cancellationToken);
            var accionesPublicadas = await dbContext.Acciones.CountAsync(a => a.Estado == Core.Enums.EstadoAccion.Publicada, cancellationToken);
            var horasTotales = await dbContext.Asistencias.SumAsync(a => a.HorasComputadas ?? 0m, cancellationToken);

            var resultado = new
            {
                voluntariosActivos,
                accionesPublicadas,
                horasTotales,
                generadoEn = DateTimeOffset.UtcNow
            };

            return Results.Ok(resultado);
        })
        .RequireAuthorization(PolicyAdminOrCoordinador)
        .WithTags("Reportes");

        return routes;
    }
}
