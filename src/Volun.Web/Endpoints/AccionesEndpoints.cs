using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Volun.Core.Entities;
using Volun.Core.Enums;
using Volun.Core.Repositories;
using Volun.Core.ValueObjects;
using Volun.Infrastructure.Persistence;
using Volun.Web.Dtos;
using Volun.Web.Mappings;

namespace Volun.Web.Endpoints;

public static class AccionesEndpoints
{
    private const string PolicyAdminOrCoordinador = "RequireAdminOrCoordinador";

    public static IEndpointRouteBuilder MapAccionesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/acciones")
            .WithTags("Acciones")
            .RequireAuthorization();

        group.MapGet("/", async Task<IResult> (
            [AsParameters] AccionQuery query,
            VolunDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var page = Math.Max(0, query.Page);
            var size = Math.Clamp(query.Size, 1, 100);

            IQueryable<Accion> accionesQuery = dbContext.Acciones
                .AsNoTracking()
                .Include(a => a.Turnos);

            if (!string.IsNullOrWhiteSpace(query.Term))
            {
                var term = query.Term.ToLower();
                accionesQuery = accionesQuery.Where(a =>
                    a.Titulo.ToLower().Contains(term) ||
                    a.Categoria.ToLower().Contains(term) ||
                    a.Ubicacion.ToLower().Contains(term));
            }

            if (query.Estado is not null)
            {
                accionesQuery = accionesQuery.Where(a => a.Estado == query.Estado);
            }

            var total = await accionesQuery.CountAsync(cancellationToken);
            var acciones = await accionesQuery
                .OrderBy(a => a.FechaInicio)
                .Skip(page * size)
                .Take(size)
                .ToListAsync(cancellationToken);

            var result = new
            {
                page,
                size,
                total,
                items = acciones.Select(a => a.ToResponse(a.CupoDisponible(0)))
            };

            return Results.Ok(result);
        })
        .AllowAnonymous();

        group.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            VolunDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var accion = await dbContext.Acciones
                .Include(a => a.Turnos)
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

            if (accion is null)
            {
                return Results.NotFound();
            }

            var cupoDisponible = accion.CupoDisponible(accion.Inscripciones.Count(i => i.Estado == EstadoInscripcion.Aprobada));
            return Results.Ok(accion.ToResponse(cupoDisponible));
        })
        .AllowAnonymous();

        group.MapPost("/", async Task<IResult> (
            CreateAccionRequest request,
            IValidator<CreateAccionRequest> validator,
            IAccionRepository repository,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var geo = request.Latitud.HasValue && request.Longitud.HasValue
                ? GeoLocation.From(request.Latitud.Value, request.Longitud.Value)
                : null;

            var accion = Accion.Create(
                request.Titulo,
                request.Descripcion,
                request.Organizador,
                request.Ubicacion,
                request.Tipo,
                request.Categoria,
                request.FechaInicio,
                request.FechaFin,
                request.CupoMaximo,
                request.Visibilidad,
                request.TurnosHabilitados,
                request.CoordinadorId,
                geo,
                request.Requisitos);

            await repository.AddAsync(accion, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/v1/acciones/{accion.Id}", accion.ToResponse(accion.CupoDisponible(0)));
        })
        .RequireAuthorization(PolicyAdminOrCoordinador);

        group.MapPut("/{id:guid}", async Task<IResult> (
            Guid id,
            UpdateAccionRequest request,
            IValidator<UpdateAccionRequest> validator,
            IAccionRepository repository,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var accion = await repository.GetByIdAsync(id, cancellationToken);
            if (accion is null)
            {
                return Results.NotFound();
            }

            var geo = request.Latitud.HasValue && request.Longitud.HasValue
                ? GeoLocation.From(request.Latitud.Value, request.Longitud.Value)
                : null;

            accion.ActualizarDetalle(
                request.Titulo,
                request.Descripcion,
                request.Ubicacion,
                request.FechaInicio,
                request.FechaFin,
                request.CupoMaximo,
                request.Visibilidad,
                request.Categoria,
                request.Requisitos,
                geo);

            await repository.UpdateAsync(accion, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Ok(accion.ToResponse(accion.CupoDisponible(accion.Inscripciones.Count(i => i.Estado == EstadoInscripcion.Aprobada))));
        })
        .RequireAuthorization(PolicyAdminOrCoordinador);

        group.MapPost("/{id:guid}/publicar", async Task<IResult> (
            Guid id,
            IAccionRepository repository,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var accion = await repository.GetByIdAsync(id, cancellationToken);
            if (accion is null)
            {
                return Results.NotFound();
            }

            accion.Publicar();
            await repository.UpdateAsync(accion, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        })
        .RequireAuthorization(PolicyAdminOrCoordinador);

        group.MapPost("/{id:guid}/turnos", async Task<IResult> (
            Guid id,
            CreateTurnoRequest request,
            IValidator<CreateTurnoRequest> validator,
            IAccionRepository repository,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var accion = await repository.GetByIdAsync(id, cancellationToken);
            if (accion is null)
            {
                return Results.NotFound();
            }

            var turno = accion.AgregarTurno(request.Titulo, request.FechaInicio, request.FechaFin, request.Cupo, request.Notas);
            await repository.UpdateAsync(accion, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/v1/acciones/{id}/turnos/{turno.Id}", turno);
        })
        .RequireAuthorization(PolicyAdminOrCoordinador);

        return routes;
    }

    public sealed record AccionQuery(int Page = 0, int Size = 20, string? Term = null, EstadoAccion? Estado = null);
}
