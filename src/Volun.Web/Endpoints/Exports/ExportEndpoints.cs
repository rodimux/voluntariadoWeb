using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Volun.Infrastructure.Persistence;
using Volun.Web.Security;

namespace Volun.Web.Endpoints.Exports;

public static class ExportEndpoints
{
    private const string PolicyAdminOrCoordinador = "RequireAdminOrCoordinador";

    public static IEndpointRouteBuilder MapExportEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/v1/export/voluntarios", async Task<IResult> (
            ClaimsPrincipal user,
            VolunDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var isAdmin = user.IsAdmin();
            var currentUserId = user.GetUserId();

            var voluntariosQuery = dbContext.Voluntarios
                .AsNoTracking()
                .AsQueryable();

            if (!isAdmin)
            {
                if (!user.IsCoordinador() || currentUserId is null)
                {
                    return Results.Forbid();
                }

                voluntariosQuery = voluntariosQuery
                    .Where(v => v.Inscripciones.Any(i => i.Accion != null && i.Accion.CoordinadorId == currentUserId));
            }

            var voluntarios = await voluntariosQuery.ToListAsync(cancellationToken);
            var builder = new StringBuilder();
            builder.AppendLine("Id,Nombre,Apellidos,Email,Pais,Provincia,FechaAlta");

            foreach (var voluntario in voluntarios)
            {
                builder.AppendLine(string.Join(',',
                    voluntario.Id,
                    Escape(voluntario.Nombre),
                    Escape(voluntario.Apellidos),
                    Escape(voluntario.Email),
                    Escape(voluntario.Pais),
                    Escape(voluntario.Provincia),
                    voluntario.FechaAlta.ToString("o", CultureInfo.InvariantCulture)));
            }

            return Results.File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "voluntarios.csv");
        })
        .RequireAuthorization(PolicyAdminOrCoordinador)
        .WithTags("Exportaciones");

        routes.MapGet("/api/v1/export/inscripciones", async Task<IResult> (
            ClaimsPrincipal user,
            VolunDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var isAdmin = user.IsAdmin();
            var currentUserId = user.GetUserId();

            var inscripcionesQuery = dbContext.Inscripciones
                .AsNoTracking()
                .Include(i => i.Voluntario)
                .Include(i => i.Accion)
                .AsQueryable();

            if (!isAdmin)
            {
                if (!user.IsCoordinador() || currentUserId is null)
                {
                    return Results.Forbid();
                }

                inscripcionesQuery = inscripcionesQuery
                    .Where(i => i.Accion != null && i.Accion.CoordinadorId == currentUserId);
            }

            var inscripciones = await inscripcionesQuery.ToListAsync(cancellationToken);
            var builder = new StringBuilder();
            builder.AppendLine("Id,Voluntario,Accion,Estado,FechaSolicitud,FechaEstado");

            foreach (var inscripcion in inscripciones)
            {
                builder.AppendLine(string.Join(',',
                    inscripcion.Id,
                    Escape($"{inscripcion.Voluntario?.Nombre} {inscripcion.Voluntario?.Apellidos}".Trim()),
                    Escape(inscripcion.Accion?.Titulo),
                    inscripcion.Estado.ToString(),
                    inscripcion.FechaSolicitud.ToString("o", CultureInfo.InvariantCulture),
                    inscripcion.FechaEstado.ToString("o", CultureInfo.InvariantCulture)));
            }

            return Results.File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "inscripciones.csv");
        })
        .RequireAuthorization(PolicyAdminOrCoordinador)
        .WithTags("Exportaciones");

        routes.MapGet("/api/v1/export/asistencias", async Task<IResult> (
            ClaimsPrincipal user,
            VolunDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var isAdmin = user.IsAdmin();
            var currentUserId = user.GetUserId();

            var asistenciasQuery = dbContext.Asistencias
                .AsNoTracking()
                .Include(a => a.Inscripcion!)
                    .ThenInclude(i => i.Accion)
                .Include(a => a.Inscripcion!)
                    .ThenInclude(i => i.Voluntario)
                .AsQueryable();

            if (!isAdmin)
            {
                if (!user.IsCoordinador() || currentUserId is null)
                {
                    return Results.Forbid();
                }

                asistenciasQuery = asistenciasQuery
                    .Where(a => a.Inscripcion != null && a.Inscripcion.Accion != null && a.Inscripcion.Accion.CoordinadorId == currentUserId);
            }

            var asistencias = await asistenciasQuery.ToListAsync(cancellationToken);
            var builder = new StringBuilder();
            builder.AppendLine("Id,Accion,Voluntario,CheckIn,CheckOut,Horas,Metodo");

            foreach (var asistencia in asistencias)
            {
                builder.AppendLine(string.Join(',',
                    asistencia.Id,
                    Escape(asistencia.Inscripcion?.Accion?.Titulo),
                    Escape($"{asistencia.Inscripcion?.Voluntario?.Nombre} {asistencia.Inscripcion?.Voluntario?.Apellidos}".Trim()),
                    asistencia.CheckIn.ToString("o", CultureInfo.InvariantCulture),
                    asistencia.CheckOut?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty,
                    asistencia.HorasComputadas?.ToString("F2", CultureInfo.InvariantCulture) ?? string.Empty,
                    asistencia.Metodo.ToString()));
            }

            return Results.File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "asistencias.csv");
        })
        .RequireAuthorization(PolicyAdminOrCoordinador)
        .WithTags("Exportaciones");

        return routes;
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains(',') || value.Contains('"'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
