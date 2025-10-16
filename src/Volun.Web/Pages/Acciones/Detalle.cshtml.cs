using System;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Volun.Core.Entities;
using Volun.Infrastructure.Persistence;

namespace Volun.Web.Pages.Acciones;

public class DetalleModel : PageModel
{
    private readonly VolunDbContext _dbContext;

    public DetalleModel(VolunDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Accion? Accion { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Accion = await _dbContext.Acciones
            .Include(a => a.Turnos)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (Accion is null)
        {
            return NotFound();
        }

        return Page();
    }
}
