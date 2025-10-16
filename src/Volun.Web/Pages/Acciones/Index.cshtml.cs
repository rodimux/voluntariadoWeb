using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Volun.Core.Entities;
using Volun.Infrastructure.Persistence;

namespace Volun.Web.Pages.Acciones;

public class IndexModel : PageModel
{
    private readonly VolunDbContext _dbContext;

    public IndexModel(VolunDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IList<Accion> Acciones { get; private set; } = new List<Accion>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Acciones = await _dbContext.Acciones
            .AsNoTracking()
            .OrderBy(a => a.FechaInicio)
            .Take(10)
            .ToListAsync(cancellationToken);
    }
}
