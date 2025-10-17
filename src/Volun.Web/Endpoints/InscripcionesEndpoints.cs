using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using Volun.Core.Entities;
using Volun.Core.Enums;
using Volun.Core.Repositories;
using Volun.Core.Services;
using Volun.Infrastructure.Persistence;
using Volun.Web.Dtos;
using Volun.Web.Mappings;
using Volun.Web.Security;

namespace Volun.Web.Endpoints;

public static class InscripcionesEndpoints
{
    private static readonly CoordinadorOwnsAccionRequirement OwnsAccionRequirement = new();

    public static IEndpointRouteBuilder MapInscripcionesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/inscripciones")
            .WithTags("Inscripciones")
            .RequireAuthorization();

        group.MapPost("/", async Task<IResult> (
            ClaimsPrincipal user,
            CreateInscripcionRequest request,
            IValidator<CreateInscripcionRequest> validator,
            IInscripcionRepository inscripcionRepository,
            IAccionRepository accionRepository,
            IVoluntarioRepository voluntarioRepository,
            [FromServices] IAuditoriaService auditoriaService,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var accion = await accionRepository.GetByIdAsync(request.AccionId, cancellationToken);
            if (accion is null)
            {
                return Results.NotFound(new ProblemDetails { Title = "Acción no encontrada" });
            }

            var voluntario = await voluntarioRepository.GetByIdAsync(request.VoluntarioId, cancellationToken);
            if (voluntario is null)
            {
                return Results.NotFound(new ProblemDetails { Title = "Voluntario no encontrado" });
            }

            if (accion.TurnosHabilitados && request.TurnoId is null)
            {
                return Results.UnprocessableEntity(new ProblemDetails
                {
                    Title = "El turno es obligatorio",
                    Detail = "La acción requiere selección de turno.",
                    Status = StatusCodes.Status422UnprocessableEntity
                });
            }

            if (!accion.TurnosHabilitados && request.TurnoId is not null)
            {
                return Results.UnprocessableEntity(new ProblemDetails
                {
                    Title = "Turno no permitido",
                    Detail = "La acción no admite turnos, no se debe especificar TurnoId.",
                    Status = StatusCodes.Status422UnprocessableEntity
                });
            }

            Turno? turnoSeleccionado = null;
            if (request.TurnoId is not null)
            {
                turnoSeleccionado = accion.Turnos.FirstOrDefault(t => t.Id == request.TurnoId.Value);
                if (turnoSeleccionado is null)
                {
                    return Results.UnprocessableEntity(new ProblemDetails
                    {
                        Title = "Turno inválido",
                        Detail = "El turno indicado no pertenece a la acción especificada.",
                        Status = StatusCodes.Status422UnprocessableEntity
                    });
                }
            }

            var existente = await inscripcionRepository.GetByVoluntarioAsync(request.VoluntarioId, request.AccionId, cancellationToken);
            if (existente is not null)
            {
                return Results.Conflict(new ProblemDetails { Title = "Inscripción existente" });
            }

            var inscripcion = Inscripcion.Create(request.VoluntarioId, request.AccionId, request.TurnoId, request.Notas);

            var aprobadasAccion = await inscripcionRepository.ContarPorAccionAsync(request.AccionId, EstadoInscripcion.Aprobada, cancellationToken);
            var sinCupoAccion = !accion.TieneCupoDisponible(aprobadasAccion);

            bool sinCupoTurno = false;
            if (turnoSeleccionado is not null && turnoSeleccionado.Cupo > 0)
            {
                var aprobadasTurno = await inscripcionRepository.ContarPorTurnoAsync(turnoSeleccionado.Id, EstadoInscripcion.Aprobada, cancellationToken);
                sinCupoTurno = aprobadasTurno >= turnoSeleccionado.Cupo;
            }

        if (sinCupoAccion || sinCupoTurno)
        {
            var motivo = sinCupoAccion && sinCupoTurno
                ? "Cupo general y de turno completo"
                : sinCupoTurno
                    ? "Cupo de turno completo"
                    : "Cupo general completo";

            inscripcion.CambiarEstado(EstadoInscripcion.ListaEspera, motivo);
            await inscripcionRepository.AddAsync(inscripcion, cancellationToken);

            var actorLista = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
            await auditoriaService.RegistrarAsync(
                nameof(Inscripcion),
                inscripcion.Id,
                "CreacionListaEspera",
                actorLista,
                new
                {
                    request.VoluntarioId,
                    request.AccionId,
                    TurnoId = request.TurnoId,
                    Motivo = motivo
                },
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Conflict(inscripcion.ToResponse());
        }

        await inscripcionRepository.AddAsync(inscripcion, cancellationToken);

        var actorCreacion = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        await auditoriaService.RegistrarAsync(
            nameof(Inscripcion),
            inscripcion.Id,
            "Creacion",
            actorCreacion,
            new
            {
                request.VoluntarioId,
                request.AccionId,
                TurnoId = request.TurnoId
            },
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/v1/inscripciones/{inscripcion.Id}", inscripcion.ToResponse());
    })
        .RequireAuthorization();

        group.MapPatch("/{id:guid}/estado", async Task<IResult> (
            ClaimsPrincipal user,
            Guid id,
            UpdateEstadoInscripcionRequest request,
            IValidator<UpdateEstadoInscripcionRequest> validator,
            IInscripcionRepository repository,
            IAuthorizationService authorizationService,
            [FromServices] IAuditoriaService auditoriaService,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var inscripcion = await repository.GetByIdAsync(id, cancellationToken);
            if (inscripcion is null)
            {
                return Results.NotFound();
            }

            if (inscripcion.Accion is null)
            {
                return Results.Problem("La inscripción no tiene acción asociada.", statusCode: StatusCodes.Status500InternalServerError);
            }

            var estadoAnterior = inscripcion.Estado;

            var authResult = await authorizationService.AuthorizeAsync(user, inscripcion.Accion, OwnsAccionRequirement);
            if (!authResult.Succeeded)
            {
                return Results.Forbid();
            }

            try
            {
                inscripcion.CambiarEstado(request.Estado, request.Comentarios);
            }
            catch (InvalidOperationException ex)
            {
                return Results.UnprocessableEntity(new ProblemDetails
                {
                    Title = "Transición inválida",
                    Detail = ex.Message,
                    Status = StatusCodes.Status422UnprocessableEntity
                });
            }

        await repository.UpdateAsync(inscripcion, cancellationToken);

        var actorCambio = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        await auditoriaService.RegistrarAsync(
            nameof(Inscripcion),
            inscripcion.Id,
            "CambioEstado",
            actorCambio,
            new
            {
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = inscripcion.Estado,
                request.Comentarios
            },
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.Ok(inscripcion.ToResponse());
        })
        .RequireAuthorization(AuthorizationPolicies.AdminOrCoordinador);

        group.MapPost("/{id:guid}/checkin", async Task<IResult> (
            ClaimsPrincipal user,
            Guid id,
            CheckInRequest request,
            IValidator<CheckInRequest> validator,
            IInscripcionRepository inscripcionRepository,
            VolunDbContext dbContext,
            IAuthorizationService authorizationService,
            [FromServices] IAuditoriaService auditoriaService,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            if (id != request.InscripcionId)
            {
                return Results.BadRequest();
            }

            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var inscripcion = await inscripcionRepository.GetByIdAsync(id, cancellationToken);
            if (inscripcion is null)
            {
                return Results.NotFound();
            }

            if (inscripcion.Accion is null)
            {
                return Results.Problem("La inscripción no tiene acción asociada.", statusCode: StatusCodes.Status500InternalServerError);
            }

            var authResult = await authorizationService.AuthorizeAsync(user, inscripcion.Accion, OwnsAccionRequirement);
            if (!authResult.Succeeded)
            {
                return Results.Forbid();
            }

            var asistencia = Asistencia.Create(id, DateTimeOffset.UtcNow, request.Metodo, request.Comentarios);
            dbContext.Asistencias.Add(asistencia);

            var actorCheckIn = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
            await auditoriaService.RegistrarAsync(
                nameof(Asistencia),
                asistencia.Id,
                "CheckIn",
                actorCheckIn,
                new
                {
                    InscripcionId = asistencia.InscripcionId,
                    request.Metodo,
                    request.Comentarios
                },
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Ok(asistencia.ToResponse());
        })
        .RequireAuthorization(AuthorizationPolicies.AdminOrCoordinador);

        group.MapPost("/{id:guid}/checkout", async Task<IResult> (
            ClaimsPrincipal user,
            Guid id,
            CheckOutRequest request,
            IValidator<CheckOutRequest> validator,
            IInscripcionRepository inscripcionRepository,
            VolunDbContext dbContext,
            IAuthorizationService authorizationService,
            [FromServices] IAuditoriaService auditoriaService,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            if (id != request.InscripcionId)
            {
                return Results.BadRequest();
            }

            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var asistencia = await dbContext.Asistencias
                .Where(a => a.InscripcionId == id)
                .OrderByDescending(a => a.CheckIn)
                .FirstOrDefaultAsync(cancellationToken);

            if (asistencia is null)
            {
                return Results.NotFound();
            }

            var inscripcion = await inscripcionRepository.GetByIdAsync(id, cancellationToken);
            if (inscripcion is null)
            {
                return Results.NotFound();
            }

            if (inscripcion.Accion is null)
            {
                return Results.Problem("La inscripción no tiene acción asociada.", statusCode: StatusCodes.Status500InternalServerError);
            }

            var authResult = await authorizationService.AuthorizeAsync(user, inscripcion.Accion, OwnsAccionRequirement);
            if (!authResult.Succeeded)
            {
                return Results.Forbid();
            }

            asistencia.RegistrarCheckOut(request.CheckOut, request.Comentarios);
            dbContext.Asistencias.Update(asistencia);

            var actorCheckOut = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
            await auditoriaService.RegistrarAsync(
                nameof(Asistencia),
                asistencia.Id,
                "CheckOut",
                actorCheckOut,
                new
                {
                    InscripcionId = asistencia.InscripcionId,
                    request.CheckOut,
                    request.Comentarios,
                    HorasComputadas = asistencia.HorasComputadas
                },
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Ok(asistencia.ToResponse());
        })
        .RequireAuthorization(AuthorizationPolicies.AdminOrCoordinador);

        group.MapGet("/{id:guid}/qr", async Task<IResult> (
            ClaimsPrincipal user,
            Guid id,
            VolunDbContext dbContext,
            IQrCodeGenerator qrCodeGenerator,
            IAuthorizationService authorizationService,
            CancellationToken cancellationToken) =>
        {
            var inscripcion = await dbContext.Inscripciones
                .Include(i => i.Accion)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

            if (inscripcion is null)
            {
                return Results.NotFound();
            }

            if (inscripcion.Accion is null)
            {
                return Results.Problem("La inscripción no tiene acción asociada.", statusCode: StatusCodes.Status500InternalServerError);
            }

            var authResult = await authorizationService.AuthorizeAsync(user, inscripcion.Accion, OwnsAccionRequirement);
            if (!authResult.Succeeded)
            {
                return Results.Forbid();
            }

            var qrBytes = qrCodeGenerator.GenerateQr(inscripcion.QrToken);
            var fileName = $"inscripcion-{inscripcion.Id}.png";
            return Results.File(qrBytes, "image/png", fileName);
        })
        .RequireAuthorization(AuthorizationPolicies.AdminOrCoordinador);

        routes.MapGet("/api/v1/mis-inscripciones/{id:guid}/qr", async Task<IResult> (
            ClaimsPrincipal user,
            Guid id,
            IInscripcionRepository inscripcionRepository,
            IQrCodeGenerator qrCodeGenerator,
            CancellationToken cancellationToken) =>
        {
            var voluntarioId = user.GetVoluntarioId();
            if (!user.IsVoluntario() || voluntarioId is null)
            {
                return Results.Forbid();
            }

            var inscripcion = await inscripcionRepository.GetByIdAsync(id, cancellationToken);
            if (inscripcion is null || inscripcion.VoluntarioId != voluntarioId)
            {
                return Results.NotFound();
            }

            var qrBytes = qrCodeGenerator.GenerateQr(inscripcion.QrToken);
            var fileName = $"mi-inscripcion-{inscripcion.Id}.png";
            return Results.File(qrBytes, "image/png", fileName);
        })
        .RequireAuthorization();

        return routes;
    }
}






