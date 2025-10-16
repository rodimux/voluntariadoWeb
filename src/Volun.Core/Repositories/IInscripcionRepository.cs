using Volun.Core.Entities;
using Volun.Core.Enums;

namespace Volun.Core.Repositories;

public interface IInscripcionRepository
{
    Task<Inscripcion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Inscripcion?> GetByVoluntarioAsync(Guid voluntarioId, Guid accionId, CancellationToken cancellationToken = default);
    Task<int> ContarPorAccionAsync(Guid accionId, EstadoInscripcion estado, CancellationToken cancellationToken = default);
    Task AddAsync(Inscripcion inscripcion, CancellationToken cancellationToken = default);
    Task UpdateAsync(Inscripcion inscripcion, CancellationToken cancellationToken = default);
}
