using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Volun.Core.Entities;
using Volun.Core.Enums;
using Volun.Core.Repositories;
using Volun.Infrastructure.Persistence;
using Volun.Web.Dtos;
using Volun.Web.Mappings;

namespace Volun.Web.Endpoints;

public static class InscripcionesEndpoints
{
    private const string PolicyAdminOrCoordinador = "RequireAdminOrCoordinador";

    public static IEndpointRouteBuilder MapInscripcionesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/inscripciones")
            .WithTags("Inscripciones")
            .RequireAuthorization();

        group.MapPost("/", async Task<IResult> (
            CreateInscripcionRequest request,
            IValidator<CreateInscripcionRequest> validator,
            IInscripcionRepository inscripcionRepository,
            IAccionRepository accionRepository,
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

            var existente = await inscripcionRepository.GetByVoluntarioAsync(request.VoluntarioId, request.AccionId, cancellationToken);
            if (existente is not null)
            {
                return Results.Conflict(new ProblemDetails { Title = "Inscripción existente" });
            }

            var inscripcion = Inscripcion.Create(request.VoluntarioId, request.AccionId, request.TurnoId, request.Notas);

            var aprobadas = await inscripcionRepository.ContarPorAccionAsync(request.AccionId, EstadoInscripcion.Aprobada, cancellationToken);
            if (!accion.TieneCupoDisponible(aprobadas))
            {
                inscripcion.CambiarEstado(EstadoInscripcion.ListaEspera, "Cupo completo");
                await inscripcionRepository.AddAsync(inscripcion, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Results.Conflict(inscripcion.ToResponse());
            }

            await inscripcionRepository.AddAsync(inscripcion, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/v1/inscripciones/{inscripcion.Id}", inscripcion.ToResponse());
        })
        .RequireAuthorization();

        group.MapPatch("/{id:guid}/estado", async Task<IResult> (
            Guid id,
            UpdateEstadoInscripcionRequest request,
            IValidator<UpdateEstadoInscripcionRequest> validator,
            IInscripcionRepository repository,
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
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Ok(inscripcion.ToResponse());
        })
        .RequireAuthorization(PolicyAdminOrCoordinador);

        group.MapPost("/{id:guid}/checkin", async Task<IResult> (
            Guid id,
            CheckInRequest request,
            IValidator<CheckInRequest> validator,
            IInscripcionRepository inscripcionRepository,
            VolunDbContext dbContext,
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

            var asistencia = Asistencia.Create(id, DateTimeOffset.UtcNow, request.Metodo, request.Comentarios);
            dbContext.Asistencias.Add(asistencia);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Ok(asistencia.ToResponse());
        })
        .RequireAuthorization(PolicyAdminOrCoordinador);

        group.MapPost("/{id:guid}/checkout", async Task<IResult> (
            Guid id,
            CheckOutRequest request,
            IValidator<CheckOutRequest> validator,
            VolunDbContext dbContext,
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

            asistencia.RegistrarCheckOut(request.CheckOut, request.Comentarios);
            dbContext.Asistencias.Update(asistencia);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Ok(asistencia.ToResponse());
        })
        .RequireAuthorization(PolicyAdminOrCoordinador);

        return routes;
    }
}
