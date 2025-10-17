using System;
using System.Threading;
using System.Threading.Tasks;

namespace Volun.Core.Services;

public interface IAuditoriaService
{
    Task RegistrarAsync(string entidad, Guid entidadId, string accion, string usuario, object datos, CancellationToken cancellationToken = default);
}
