using System.Linq.Expressions;
using Volun.Core.Entities;

namespace Volun.Core.Repositories;

public interface IVoluntarioRepository
{
    Task<Voluntario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Voluntario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Voluntario>> SearchAsync(
        Expression<Func<Voluntario, bool>> predicate,
        int page,
        int size,
        CancellationToken cancellationToken = default);
    Task AddAsync(Voluntario voluntario, CancellationToken cancellationToken = default);
    Task UpdateAsync(Voluntario voluntario, CancellationToken cancellationToken = default);
}
