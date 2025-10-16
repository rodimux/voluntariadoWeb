using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using Volun.Core.Entities;
using Volun.Core.Repositories;

namespace Volun.Infrastructure.Persistence.Repositories;

public class AccionRepository(VolunDbContext context) : IAccionRepository
{
    public async Task AddAsync(Accion accion, CancellationToken cancellationToken = default)
    {
        await context.Acciones.AddAsync(accion, cancellationToken);
    }

    public async Task<Accion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Acciones
            .Include(a => a.Turnos)
            .Include(a => a.Inscripciones)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Accion>> SearchAsync(
        Expression<Func<Accion, bool>> predicate,
        int page,
        int size,
        CancellationToken cancellationToken = default)
    {
        var query = context.Acciones
            .Include(a => a.Turnos)
            .Where(predicate)
            .OrderByDescending(a => a.FechaInicio);

        return await query
            .Skip(page * size)
            .Take(size)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(Accion accion, CancellationToken cancellationToken = default)
    {
        context.Acciones.Update(accion);
        return Task.CompletedTask;
    }
}
