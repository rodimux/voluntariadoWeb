using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Volun.Core.Enums;
using Volun.Infrastructure.Persistence;
using Volun.Web.Security;

namespace Volun.Web.Endpoints.Admin;

public static class ReportesEndpoints
{
    private const string PolicyAdminOrCoordinador = "RequireAdminOrCoordinador";

    public static IEndpointRouteBuilder MapReportesEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/v1/reportes/dashboard", async Task<IResult> (
            ClaimsPrincipal user,
            VolunDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var isAdmin = user.IsAdmin();
            var currentUserId = user.GetUserId();

            var voluntariosQuery = dbContext.Voluntarios.AsQueryable();
            var accionesQuery = dbContext.Acciones.AsQueryable();
            var inscripcionesQuery = dbContext.Inscripciones.AsQueryable();
            var asistenciasQuery = dbContext.Asistencias.AsQueryable();

            if (!isAdmin)
            {
                if (!user.IsCoordinador() || currentUserId is null)
                {
                    return Results.Forbid();
                }

                accionesQuery = accionesQuery.Where(a => a.CoordinadorId == currentUserId);
                inscripcionesQuery = inscripcionesQuery.Where(i => i.Accion != null && i.Accion.CoordinadorId == currentUserId);
                asistenciasQuery = asistenciasQuery.Where(a => a.Inscripcion != null && a.Inscripcion.AccionId != Guid.Empty && a.Inscripcion.Accion != null && a.Inscripcion.Accion.CoordinadorId == currentUserId);
                voluntariosQuery = voluntariosQuery.Where(v => v.Inscripciones.Any(i => i.Accion != null && i.Accion.CoordinadorId == currentUserId));
            }

            var cutoffVoluntarios = DateTimeOffset.UtcNow.AddMonths(-12);
            var voluntariosActivos = await voluntariosQuery.CountAsync(v =>
                v.Inscripciones.Any(i => i.Estado == EstadoInscripcion.Completada && i.FechaEstado >= cutoffVoluntarios), cancellationToken);

            var accionesPublicadas = await accionesQuery.CountAsync(a => a.Estado == EstadoAccion.Publicada, cancellationToken);
            var horasTotales = await asistenciasQuery.SumAsync(a => a.HorasComputadas ?? 0m, cancellationToken);

            var inicioPeriodo = DateTimeOffset.UtcNow.AddMonths(-5);
            var accionesPorMes = await accionesQuery
                .Where(a => a.FechaInicio >= inicioPeriodo)
                .GroupBy(a => new { a.FechaInicio.Year, a.FechaInicio.Month })
                .Select(g => new
                {
                    Mes = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("yyyy-MM", CultureInfo.InvariantCulture),
                    Total = g.Count()
                })
                .OrderBy(x => x.Mes)
                .ToListAsync(cancellationToken);

            var inscripcionesPorEstado = await inscripcionesQuery
                .GroupBy(i => i.Estado)
                .Select(g => new
                {
                    Estado = g.Key.ToString(),
                    Total = g.Count()
                })
                .ToListAsync(cancellationToken);

            var topCategorias = await accionesQuery
                .Where(a => !string.IsNullOrWhiteSpace(a.Categoria))
                .GroupBy(a => a.Categoria)
                .Select(g => new
                {
                    Categoria = g.Key,
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToListAsync(cancellationToken);

            var resultado = new
            {
                voluntariosActivos,
                accionesPublicadas,
                horasTotales,
                accionesPorMes,
                inscripcionesPorEstado,
                topCategorias,
                generadoEn = DateTimeOffset.UtcNow
            };

            return Results.Ok(resultado);
        })
        .RequireAuthorization(PolicyAdminOrCoordinador)
        .WithTags("Reportes");

        return routes;
    }
}
