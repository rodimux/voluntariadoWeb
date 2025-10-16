using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Volun.Core.Enums;
using Volun.Infrastructure.Persistence;

namespace Volun.Web.Pages.Panel;

[Authorize(Roles = "Admin,Coordinador")]
public class IndexModel : PageModel
{
    private readonly VolunDbContext _dbContext;

    public IndexModel(VolunDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public int AccionesActivas { get; private set; }
    public int VoluntariosActivos { get; private set; }
    public int InscripcionesPendientes { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        AccionesActivas = await _dbContext.Acciones.CountAsync(a => a.Estado == EstadoAccion.Publicada, cancellationToken);
        VoluntariosActivos = await _dbContext.Voluntarios.CountAsync(v => v.EstaActivo, cancellationToken);
        InscripcionesPendientes = await _dbContext.Inscripciones.CountAsync(i => i.Estado == EstadoInscripcion.Pendiente, cancellationToken);
    }
}
