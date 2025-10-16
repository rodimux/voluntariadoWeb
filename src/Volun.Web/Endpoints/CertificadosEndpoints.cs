using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Volun.Core.Entities;
using Volun.Core.Enums;
using Volun.Core.Services;
using Volun.Infrastructure.Persistence;
using Volun.Web.Dtos;
using Volun.Web.Mappings;
using Volun.Web.Security;

namespace Volun.Web.Endpoints;

public static class CertificadosEndpoints
{
    private const string PolicyAdminOrCoordinador = "RequireAdminOrCoordinador";

    public static IEndpointRouteBuilder MapCertificadosEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/certificados")
            .WithTags("Certificados")
            .RequireAuthorization(PolicyAdminOrCoordinador);

        group.MapPost("/", async Task<IResult> (
            ClaimsPrincipal user,
            CreateCertificadoRequest request,
            IValidator<CreateCertificadoRequest> validator,
            VolunDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var accion = await dbContext.Acciones
                .Include(a => a.Inscripciones)
                .FirstOrDefaultAsync(a => a.Id == request.AccionId, cancellationToken);
            if (accion is null)
            {
                return Results.NotFound(new ProblemDetails { Title = "Acción no encontrada" });
            }

            var isAdmin = user.IsAdmin();
            var currentUserId = user.GetUserId();
            if (!isAdmin)
            {
                if (!user.IsCoordinador() || currentUserId is null || accion.CoordinadorId != currentUserId)
                {
                    return Results.Forbid();
                }
            }

            var voluntario = await dbContext.Voluntarios.FindAsync(new object[] { request.VoluntarioId }, cancellationToken);
            if (voluntario is null)
            {
                return Results.NotFound(new ProblemDetails { Title = "Voluntario no encontrado" });
            }

            var participacion = accion.Inscripciones.FirstOrDefault(i => i.VoluntarioId == request.VoluntarioId);
            if (participacion is null)
            {
                return Results.BadRequest(new ProblemDetails { Title = "El voluntario no está inscrito en la acción." });
            }

            if (participacion.Estado != EstadoInscripcion.Completada && participacion.Estado != EstadoInscripcion.Aprobada)
            {
                return Results.BadRequest(new ProblemDetails { Title = "La inscripción no se encuentra en estado elegible para certificar." });
            }

            var codigo = $"CERT-{Guid.NewGuid():N}";
            var urlPublica = $"/api/v1/public/certificados/{codigo}";
            var certificado = Certificado.Emitir(request.VoluntarioId, accion.Id, request.Horas, codigo, urlPublica);

            dbContext.Certificados.Add(certificado);
            await dbContext.SaveChangesAsync(cancellationToken);

            var certificadoCompleto = await dbContext.Certificados
                .Include(c => c.Voluntario)
                .Include(c => c.Accion)
                .FirstAsync(c => c.Id == certificado.Id, cancellationToken);

            return Results.Created($"/api/v1/certificados/{certificado.Id}", certificadoCompleto.ToResponse());
        });

        group.MapGet("/{id:guid}", async Task<IResult> (
            ClaimsPrincipal user,
            Guid id,
            VolunDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var certificado = await dbContext.Certificados
                .Include(c => c.Accion)
                .Include(c => c.Voluntario)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (certificado is null)
            {
                return Results.NotFound();
            }

            var isAdmin = user.IsAdmin();
            if (!isAdmin)
            {
                var currentUserId = user.GetUserId();
                if (!user.IsCoordinador() || currentUserId is null || certificado.Accion?.CoordinadorId != currentUserId)
                {
                    return Results.Forbid();
                }
            }

            return Results.Ok(certificado.ToResponse());
        });

        group.MapGet("/{id:guid}/pdf", async Task<IResult> (
            ClaimsPrincipal user,
            Guid id,
            VolunDbContext dbContext,
            ICertificateService certificateService,
            CancellationToken cancellationToken) =>
        {
            var certificado = await dbContext.Certificados
                .Include(c => c.Voluntario)
                .Include(c => c.Accion)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (certificado is null)
            {
                return Results.NotFound();
            }

            var isAdmin = user.IsAdmin();
            if (!isAdmin)
            {
                var currentUserId = user.GetUserId();
                if (!user.IsCoordinador() || currentUserId is null || certificado.Accion?.CoordinadorId != currentUserId)
                {
                    return Results.Forbid();
                }
            }

            var pdfBytes = await certificateService.GenerateCertificatePdfAsync(certificado, cancellationToken);
            var fileName = $"certificado-{certificado.CodigoVerificacion}.pdf";
            return Results.File(pdfBytes, "application/pdf", fileName);
        });

        return routes;
    }
}
