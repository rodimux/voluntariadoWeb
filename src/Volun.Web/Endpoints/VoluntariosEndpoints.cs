using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using Volun.Core.Entities;
using Volun.Core.Repositories;
using Volun.Core.Services;
using Volun.Infrastructure.Identity;
using Volun.Web.Dtos;
using Volun.Web.Mappings;
using Volun.Web.Security;

namespace Volun.Web.Endpoints;

public static class VoluntariosEndpoints
{
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
        .RequireAuthorization(AuthorizationPolicies.AdminOrCoordinador);

        group.MapGet("/{id:guid}", async Task<IResult> (
            ClaimsPrincipal user,
            Guid id,
            IVoluntarioRepository repository,
            CancellationToken cancellationToken) =>
        {
            var voluntario = await repository.GetByIdAsync(id, cancellationToken);
            if (voluntario is null)
            {
                return Results.NotFound();
            }

            if (!user.CanAccessVoluntario(voluntario.Id))
            {
                return Results.Forbid();
            }

            return Results.Ok(voluntario.ToResponse());
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
            ClaimsPrincipal user,
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

            if (!user.CanAccessVoluntario(voluntario.Id))
            {
                return Results.Forbid();
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
            ClaimsPrincipal user,
            Guid id,
            IVoluntarioRepository repository,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] IAuditoriaService auditoriaService,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var voluntario = await repository.GetByIdAsync(id, cancellationToken);
            if (voluntario is null)
            {
                return Results.NotFound();
            }

            var esAdmin = user.IsAdmin();
            var voluntarioActualId = user.GetVoluntarioId();
            if (!esAdmin && voluntarioActualId != voluntario.Id)
            {
                return Results.Forbid();
            }

            var correoAnterior = voluntario.Email;

            voluntario.Anonimizar();
            await repository.UpdateAsync(voluntario, cancellationToken);

            var usuarioSistema = await userManager.Users
                .FirstOrDefaultAsync(u => u.VoluntarioId == voluntario.Id, cancellationToken);

            if (usuarioSistema is not null)
            {
                usuarioSistema.IsActive = false;
                usuarioSistema.Email = voluntario.Email;
                usuarioSistema.UserName = voluntario.Email;
                usuarioSistema.NormalizedEmail = voluntario.Email.ToUpperInvariant();
                usuarioSistema.NormalizedUserName = usuarioSistema.NormalizedEmail;

                var updateResult = await userManager.UpdateAsync(usuarioSistema);
                if (!updateResult.Succeeded)
                {
                    return Results.Problem("No se pudo actualizar el usuario vinculado.", statusCode: StatusCodes.Status500InternalServerError);
                }
            }

            var actor = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";

            await auditoriaService.RegistrarAsync(
                nameof(Voluntario),
                voluntario.Id,
                "Anonimizacion",
                actor,
                new
                {
                    PreviousEmail = correoAnterior,
                    PerformedBy = actor,
                    PerformedAt = DateTimeOffset.UtcNow,
                    IsAdmin = esAdmin
                },
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        })
        .RequireAuthorization();

        return routes;
    }

    public sealed record PaginationQuery(int Page = 0, int Size = 20);
}
