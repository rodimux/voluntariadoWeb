using Microsoft.EntityFrameworkCore;
using Volun.Core.Entities;
using Volun.Core.Enums;
using Volun.Core.Repositories;

namespace Volun.Infrastructure.Persistence.Repositories;

public class InscripcionRepository(VolunDbContext context) : IInscripcionRepository
{
    public async Task AddAsync(Inscripcion inscripcion, CancellationToken cancellationToken = default)
    {
        await context.Inscripciones.AddAsync(inscripcion, cancellationToken);
    }

    public async Task<int> ContarPorAccionAsync(Guid accionId, EstadoInscripcion estado, CancellationToken cancellationToken = default)
        => await context.Inscripciones.CountAsync(i => i.AccionId == accionId && i.Estado == estado, cancellationToken);

    public async Task<Inscripcion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Inscripciones
            .Include(i => i.Accion)
            .Include(i => i.Voluntario)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<Inscripcion?> GetByVoluntarioAsync(Guid voluntarioId, Guid accionId, CancellationToken cancellationToken = default)
        => await context.Inscripciones
            .FirstOrDefaultAsync(i => i.VoluntarioId == voluntarioId && i.AccionId == accionId, cancellationToken);

    public Task UpdateAsync(Inscripcion inscripcion, CancellationToken cancellationToken = default)
    {
        context.Inscripciones.Update(inscripcion);
        return Task.CompletedTask;
    }
}
