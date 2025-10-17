using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using Volun.Core.Enums;
using Volun.Core.Services;
using Volun.Infrastructure.Persistence;
using Volun.Web.Security;

namespace Volun.Web.Endpoints.Exports;

public static class ExportEndpoints
{

    public static IEndpointRouteBuilder MapExportEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/v1/export/voluntarios", async Task<IResult> (
                ClaimsPrincipal user,
                VolunDbContext dbContext,
                DateOnly? from,
                DateOnly? to,
                bool? activos,
                string? format,
                [FromServices] IAuditoriaService auditoriaService,
                CancellationToken cancellationToken) =>
            {
                var formatValidation = ValidateFormat(format, out _);
                if (formatValidation is not null)
                {
                    return formatValidation;
                }

                var query = dbContext.Voluntarios.AsNoTracking().AsQueryable();

                if (from.HasValue)
                {
                    var filterFrom = ToDateTimeOffset(from.Value, endOfDay: false);
                    query = query.Where(v => v.FechaAlta >= filterFrom);
                }

                if (to.HasValue)
                {
                    var filterTo = ToDateTimeOffset(to.Value, endOfDay: true);
                    query = query.Where(v => v.FechaAlta <= filterTo);
                }

                if (activos.HasValue)
                {
                    query = query.Where(v => v.EstaActivo == activos.Value);
                }

                var voluntarios = await query
                    .OrderBy(v => v.Apellidos)
                    .ThenBy(v => v.Nombre)
                    .ToListAsync(cancellationToken);

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

                var actor = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
                await auditoriaService.RegistrarAsync(
                    "ExportVoluntarios",
                    Guid.Empty,
                    "ExportCsv",
                    actor,
                    new
                    {
                        From = from,
                        To = to,
                        Activos = activos,
                        Count = voluntarios.Count
                    },
                    cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "voluntarios.csv");
            })
            .RequireAuthorization(AuthorizationPolicies.AdminOrCoordinador)
            .WithTags("Exportaciones");

        routes.MapGet("/api/v1/export/inscripciones", async Task<IResult> (
                ClaimsPrincipal user,
                VolunDbContext dbContext,
                DateOnly? from,
                DateOnly? to,
                Guid? accionId,
                Guid? voluntarioId,
                EstadoInscripcion? estado,
                string? format,
                [FromServices] IAuditoriaService auditoriaService,
                CancellationToken cancellationToken) =>
            {
                var formatValidation = ValidateFormat(format, out _);
                if (formatValidation is not null)
                {
                    return formatValidation;
                }

                var query = dbContext.Inscripciones
                    .AsNoTracking()
                    .Include(i => i.Voluntario)
                    .Include(i => i.Accion)
                    .Include(i => i.Turno)
                    .AsQueryable();

                if (from.HasValue)
                {
                    var filterFrom = ToDateTimeOffset(from.Value, endOfDay: false);
                    query = query.Where(i => i.FechaSolicitud >= filterFrom);
                }

                if (to.HasValue)
                {
                    var filterTo = ToDateTimeOffset(to.Value, endOfDay: true);
                    query = query.Where(i => i.FechaSolicitud <= filterTo);
                }

                if (accionId.HasValue)
                {
                    query = query.Where(i => i.AccionId == accionId.Value);
                }

                if (voluntarioId.HasValue)
                {
                    query = query.Where(i => i.VoluntarioId == voluntarioId.Value);
                }

                if (estado.HasValue)
                {
                    query = query.Where(i => i.Estado == estado.Value);
                }

                var inscripciones = await query
                    .OrderBy(i => i.FechaSolicitud)
                    .ThenBy(i => i.Id)
                    .ToListAsync(cancellationToken);

                var builder = new StringBuilder();
                builder.AppendLine("Id,VoluntarioId,VoluntarioNombre,VoluntarioApellidos,VoluntarioEmail,AccionId,AccionTitulo,TurnoId,TurnoTitulo,Estado,FechaSolicitud,FechaEstado");

                foreach (var inscripcion in inscripciones)
                {
                    builder.AppendLine(string.Join(',',
                        inscripcion.Id,
                        inscripcion.VoluntarioId,
                        Escape(inscripcion.Voluntario?.Nombre),
                        Escape(inscripcion.Voluntario?.Apellidos),
                        Escape(inscripcion.Voluntario?.Email),
                        inscripcion.AccionId,
                        Escape(inscripcion.Accion?.Titulo),
                        inscripcion.TurnoId,
                        Escape(inscripcion.Turno?.Titulo),
                        inscripcion.Estado,
                        inscripcion.FechaSolicitud.ToString("o", CultureInfo.InvariantCulture),
                        inscripcion.FechaEstado.ToString("o", CultureInfo.InvariantCulture)));
                }

                var actor = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
                await auditoriaService.RegistrarAsync(
                    "ExportInscripciones",
                    Guid.Empty,
                    "ExportCsv",
                    actor,
                    new
                    {
                        From = from,
                        To = to,
                        AccionId = accionId,
                        VoluntarioId = voluntarioId,
                        Estado = estado,
                        Count = inscripciones.Count
                    },
                    cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "inscripciones.csv");
            })
            .RequireAuthorization(AuthorizationPolicies.AdminOrCoordinador)
            .WithTags("Exportaciones");

        routes.MapGet("/api/v1/export/asistencias", async Task<IResult> (
                ClaimsPrincipal user,
                VolunDbContext dbContext,
                DateOnly? from,
                DateOnly? to,
                Guid? accionId,
                Guid? voluntarioId,
                MetodoAsistencia? metodo,
                string? format,
                [FromServices] IAuditoriaService auditoriaService,
                CancellationToken cancellationToken) =>
            {
                var formatValidation = ValidateFormat(format, out _);
                if (formatValidation is not null)
                {
                    return formatValidation;
                }

                var query = dbContext.Asistencias
                    .AsNoTracking()
                    .Include(a => a.Inscripcion)
                        .ThenInclude(i => i!.Voluntario)
                    .Include(a => a.Inscripcion)
                        .ThenInclude(i => i!.Accion)
                    .Include(a => a.Inscripcion)
                        .ThenInclude(i => i!.Turno)
                    .AsQueryable();

                if (from.HasValue)
                {
                    var filterFrom = ToDateTimeOffset(from.Value, endOfDay: false);
                    query = query.Where(a => a.CheckIn >= filterFrom);
                }

                if (to.HasValue)
                {
                    var filterTo = ToDateTimeOffset(to.Value, endOfDay: true);
                    query = query.Where(a => a.CheckIn <= filterTo);
                }

                if (accionId.HasValue)
                {
                    query = query.Where(a => a.Inscripcion != null && a.Inscripcion!.AccionId == accionId.Value);
                }

                if (voluntarioId.HasValue)
                {
                    query = query.Where(a => a.Inscripcion != null && a.Inscripcion!.VoluntarioId == voluntarioId.Value);
                }

                if (metodo.HasValue)
                {
                    query = query.Where(a => a.Metodo == metodo.Value);
                }

                var asistencias = await query
                    .OrderBy(a => a.CheckIn)
                    .ThenBy(a => a.Id)
                    .ToListAsync(cancellationToken);

                var builder = new StringBuilder();
                builder.AppendLine("Id,InscripcionId,VoluntarioId,VoluntarioNombre,VoluntarioApellidos,VoluntarioEmail,AccionId,AccionTitulo,TurnoId,TurnoTitulo,CheckIn,CheckOut,HorasComputadas,Metodo,Comentarios");

                foreach (var asistencia in asistencias)
                {
                    builder.AppendLine(string.Join(',',
                        asistencia.Id,
                        asistencia.InscripcionId,
                        asistencia.Inscripcion?.VoluntarioId,
                        Escape(asistencia.Inscripcion?.Voluntario?.Nombre),
                        Escape(asistencia.Inscripcion?.Voluntario?.Apellidos),
                        Escape(asistencia.Inscripcion?.Voluntario?.Email),
                        asistencia.Inscripcion?.AccionId,
                        Escape(asistencia.Inscripcion?.Accion?.Titulo),
                        asistencia.Inscripcion?.TurnoId,
                        Escape(asistencia.Inscripcion?.Turno?.Titulo),
                        asistencia.CheckIn.ToString("o", CultureInfo.InvariantCulture),
                        asistencia.CheckOut?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty,
                        asistencia.HorasComputadas?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                        asistencia.Metodo,
                        Escape(asistencia.Comentarios)));
                }

                var actor = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
                await auditoriaService.RegistrarAsync(
                    "ExportAsistencias",
                    Guid.Empty,
                    "ExportCsv",
                    actor,
                    new
                    {
                        From = from,
                        To = to,
                        AccionId = accionId,
                        VoluntarioId = voluntarioId,
                        Metodo = metodo,
                        Count = asistencias.Count
                    },
                    cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "asistencias.csv");
            })
            .RequireAuthorization(AuthorizationPolicies.AdminOrCoordinador)
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

    private static IResult? ValidateFormat(string? format, out string selectedFormat)
    {
        selectedFormat = string.IsNullOrWhiteSpace(format)
            ? "csv"
            : format.Trim().ToLowerInvariant();

        if (selectedFormat == "xlsx")
        {
            return Results.StatusCode(501);
        }

        if (selectedFormat != "csv")
        {
            return Results.BadRequest(new
            {
                Error = "Formato no soportado. Usa format=csv o format=xlsx."
            });
        }

        return null;
    }

    private static DateTimeOffset ToDateTimeOffset(DateOnly date, bool endOfDay)
    {
        var time = endOfDay ? TimeOnly.MaxValue : TimeOnly.MinValue;
        var dateTime = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Utc);
        return new DateTimeOffset(dateTime);
    }
}
