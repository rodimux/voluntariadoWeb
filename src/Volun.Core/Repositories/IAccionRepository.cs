using System.Linq.Expressions;
using Volun.Core.Entities;

namespace Volun.Core.Repositories;

public interface IAccionRepository
{
    Task<Accion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Accion>> SearchAsync(
        Expression<Func<Accion, bool>> predicate,
        int page,
        int size,
        CancellationToken cancellationToken = default);
    Task AddAsync(Accion accion, CancellationToken cancellationToken = default);
    Task UpdateAsync(Accion accion, CancellationToken cancellationToken = default);
}
