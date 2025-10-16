using Microsoft.EntityFrameworkCore;
using System.Linq;
using Volun.Core.Entities;
using Volun.Core.Repositories;

namespace Volun.Infrastructure.Persistence.Repositories;

public class VoluntarioRepository(VolunDbContext context) : IVoluntarioRepository
{
    public async Task AddAsync(Voluntario voluntario, CancellationToken cancellationToken = default)
    {
        await context.Voluntarios.AddAsync(voluntario, cancellationToken);
    }

    public async Task<Voluntario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await context.Voluntarios
            .Include(v => v.Inscripciones)
            .FirstOrDefaultAsync(v => v.Email == email, cancellationToken);

    public async Task<Voluntario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Voluntarios
            .Include(v => v.Inscripciones)
                .ThenInclude(i => i.Accion)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Voluntario>> SearchAsync(
        System.Linq.Expressions.Expression<Func<Voluntario, bool>> predicate,
        int page,
        int size,
        CancellationToken cancellationToken = default)
    {
        var query = context.Voluntarios
            .Where(predicate)
            .OrderBy(v => v.Apellidos)
            .ThenBy(v => v.Nombre);

        return await query
            .Skip(page * size)
            .Take(size)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(Voluntario voluntario, CancellationToken cancellationToken = default)
    {
        context.Voluntarios.Update(voluntario);
        return Task.CompletedTask;
    }
}
