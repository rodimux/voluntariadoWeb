using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using Volun.Infrastructure.Persistence;

namespace Volun.Web.Endpoints.Exports;

public static class ExportEndpoints
{
    private const string PolicyAdminOrCoordinador = "RequireAdminOrCoordinador";

    public static IEndpointRouteBuilder MapExportEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/v1/export/voluntarios", async Task<IResult> (
            VolunDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var voluntarios = await dbContext.Voluntarios.AsNoTracking().ToListAsync(cancellationToken);
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
