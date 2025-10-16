using Microsoft.EntityFrameworkCore;
using Volun.Infrastructure.Persistence;
using Volun.Web.Dtos;
using Volun.Web.Mappings;

namespace Volun.Web.Endpoints.Public;

public static class CertificadosPublicEndpoints
{
    public static IEndpointRouteBuilder MapCertificadosPublicEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/public/certificados")
            .WithTags("Certificados Públicos");

        group.MapGet("/{codigo}", async Task<IResult> (
            string codigo,
            VolunDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var certificado = await dbContext.Certificados
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CodigoVerificacion == codigo, cancellationToken);

            return certificado is null
                ? Results.NotFound()
                : Results.Ok(certificado.ToResponse());
        })
        .AllowAnonymous();

        return routes;
    }
}
