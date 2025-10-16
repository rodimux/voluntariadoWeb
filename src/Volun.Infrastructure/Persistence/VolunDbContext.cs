using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volun.Core.Entities;
using Volun.Infrastructure.Identity;

namespace Volun.Infrastructure.Persistence;

public class VolunDbContext(DbContextOptions<VolunDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public const string DefaultSchema = "volun";

    public DbSet<Voluntario> Voluntarios => Set<Voluntario>();
    public DbSet<Accion> Acciones => Set<Accion>();
    public DbSet<Turno> Turnos => Set<Turno>();
    public DbSet<Inscripcion> Inscripciones => Set<Inscripcion>();
    public DbSet<Asistencia> Asistencias => Set<Asistencia>();
    public DbSet<Certificado> Certificados => Set<Certificado>();
    public DbSet<UsuarioSistema> UsuariosSistema => Set<UsuarioSistema>();
    public DbSet<AuditoriaRegistro> Auditoria => Set<AuditoriaRegistro>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema(DefaultSchema);
        builder.ApplyConfigurationsFromAssembly(typeof(VolunDbContext).Assembly);
    }
}
