using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Volun.Core.Entities;
using Volun.Core.Services;
using Volun.Infrastructure.Persistence;

namespace Volun.Infrastructure.Services;

public class AuditoriaService(VolunDbContext dbContext) : IAuditoriaService
{
    public Task RegistrarAsync(string entidad, Guid entidadId, string accion, string usuario, object datos, CancellationToken cancellationToken = default)
    {
        var payload = datos switch
        {
            string text => text,
            _ => JsonSerializer.Serialize(datos)
        };

        var registro = AuditoriaRegistro.Crear(entidad, entidadId, accion, usuario, payload);
        dbContext.Auditoria.Add(registro);
        return Task.CompletedTask;
    }
}
