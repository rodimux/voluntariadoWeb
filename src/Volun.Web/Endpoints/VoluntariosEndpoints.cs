using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Volun.Core.Entities;
using Volun.Core.Repositories;
using Volun.Web.Dtos;
using Volun.Web.Mappings;

namespace Volun.Web.Endpoints;

public static class VoluntariosEndpoints
{
    private const string PolicyAdminOrCoordinador = "RequireAdminOrCoordinador";

    public static IEndpointRouteBuilder MapVoluntariosEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/voluntarios")
            .WithTags("Voluntarios");

        group.MapGet("/", async Task<IResult> (
            [AsParameters] PaginationQuery query,
            IVoluntarioRepository repository,
            CancellationToken cancellationToken) =>
        {
            var items = await repository.SearchAsync(_ => true, query.Page, query.Size, cancellationToken);
            var result = items.Select(v => v.ToResponse());
            return Results.Ok(new { query.Page, query.Size, items = result });
        })
        .RequireAuthorization(PolicyAdminOrCoordinador);

        group.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            IVoluntarioRepository repository,
            CancellationToken cancellationToken) =>
        {
            var voluntario = await repository.GetByIdAsync(id, cancellationToken);
            return voluntario is null ? Results.NotFound() : Results.Ok(voluntario.ToResponse());
        })
        .RequireAuthorization();

        group.MapPost("/", async Task<IResult> (
            CreateVoluntarioRequest request,
            IValidator<CreateVoluntarioRequest> validator,
            IVoluntarioRepository repository,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var existente = await repository.GetByEmailAsync(request.Email.ToLowerInvariant(), cancellationToken);
            if (existente != null)
            {
                return Results.Conflict(new ProblemDetails
                {
                    Title = "Email ya registrado",
                    Status = StatusCodes.Status409Conflict,
                    Detail = "El email proporcionado ya se encuentra registrado."
                });
            }

            var voluntario = Voluntario.Create(
                request.Nombre,
                request.Apellidos,
                request.Email,
                request.FechaNacimiento,
                request.ConsentimientoRgpd,
                DateTimeOffset.UtcNow,
                request.Telefono,
                request.DniNie,
                request.Direccion,
                request.Provincia,
                request.Pais,
                request.Disponibilidad);

            if (request.Preferencias != null)
            {
                voluntario.EstablecerPreferencias(request.Preferencias);
            }

            if (request.Habilidades != null)
            {
                voluntario.EstablecerHabilidades(request.Habilidades);
            }

            await repository.AddAsync(voluntario, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/v1/voluntarios/{voluntario.Id}", voluntario.ToResponse());
        })
        .AllowAnonymous();

        group.MapPut("/{id:guid}", async Task<IResult> (
            Guid id,
            UpdateVoluntarioRequest request,
            IValidator<UpdateVoluntarioRequest> validator,
            IVoluntarioRepository repository,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var voluntario = await repository.GetByIdAsync(id, cancellationToken);
            if (voluntario is null)
            {
                return Results.NotFound();
            }

            voluntario.ActualizarPerfil(
                request.Nombre,
                request.Apellidos,
                request.Telefono,
                request.Direccion,
                request.Provincia,
                request.Pais,
                request.Disponibilidad);

            if (request.Preferencias != null)
            {
                voluntario.EstablecerPreferencias(request.Preferencias);
            }

            if (request.Habilidades != null)
            {
                voluntario.EstablecerHabilidades(request.Habilidades);
            }

            await repository.UpdateAsync(voluntario, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Ok(voluntario.ToResponse());
        })
        .RequireAuthorization();

        group.MapDelete("/{id:guid}", async Task<IResult> (
            Guid id,
            IVoluntarioRepository repository,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var voluntario = await repository.GetByIdAsync(id, cancellationToken);
            if (voluntario is null)
            {
                return Results.NotFound();
            }

            voluntario.Suspender();
            await repository.UpdateAsync(voluntario, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        })
        .RequireAuthorization(PolicyAdminOrCoordinador);

        return routes;
    }

    public sealed record PaginationQuery(int Page = 0, int Size = 20);
}
