using Volun.Core.Repositories;

namespace Volun.Infrastructure.Persistence;

public class UnitOfWork(VolunDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
