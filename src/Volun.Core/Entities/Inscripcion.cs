using Volun.Core.Enums;

namespace Volun.Core.Entities;

public class Inscripcion : BaseEntity
{
    private static readonly HashSet<(EstadoInscripcion From, EstadoInscripcion To)> TransicionesPermitidas =
    [
        (EstadoInscripcion.Pendiente, EstadoInscripcion.Aprobada),
        (EstadoInscripcion.Pendiente, EstadoInscripcion.ListaEspera),
        (EstadoInscripcion.Pendiente, EstadoInscripcion.Cancelada),
        (EstadoInscripcion.Aprobada, EstadoInscripcion.Cancelada),
        (EstadoInscripcion.Aprobada, EstadoInscripcion.NoShow),
        (EstadoInscripcion.Aprobada, EstadoInscripcion.Completada),
        (EstadoInscripcion.ListaEspera, EstadoInscripcion.Aprobada),
        (EstadoInscripcion.ListaEspera, EstadoInscripcion.Cancelada)
    ];

    private Inscripcion()
    {
        // Requerido por EF Core
    }

    private Inscripcion(Guid voluntarioId, Guid accionId, Guid? turnoId, string? notas)
    {
        VoluntarioId = voluntarioId;
        AccionId = accionId;
        TurnoId = turnoId;
        Notas = notas;
        Estado = EstadoInscripcion.Pendiente;
        FechaSolicitud = DateTimeOffset.UtcNow;
        FechaEstado = FechaSolicitud;
        QrToken = Guid.NewGuid().ToString("N");
    }

    public Guid VoluntarioId { get; private set; }
    public Guid AccionId { get; private set; }
    public Guid? TurnoId { get; private set; }
    public EstadoInscripcion Estado { get; private set; }
    public DateTimeOffset FechaSolicitud { get; private set; }
    public DateTimeOffset FechaEstado { get; private set; }
    public string? Notas { get; private set; }
    public string? ComentariosEstado { get; private set; }
    public string QrToken { get; private set; } = null!;

    public Voluntario? Voluntario { get; private set; }
    public Accion? Accion { get; private set; }
    public Turno? Turno { get; private set; }
    public ICollection<Asistencia> Asistencias { get; private set; } = new List<Asistencia>();

    public static Inscripcion Create(Guid voluntarioId, Guid accionId, Guid? turnoId, string? notas = null)
        => new(voluntarioId, accionId, turnoId, notas);

    public void CambiarEstado(EstadoInscripcion nuevoEstado, string? comentarios = null)
    {
        if (Estado == nuevoEstado)
        {
            return;
        }

        if (!TransicionesPermitidas.Contains((Estado, nuevoEstado)))
        {
            throw new InvalidOperationException($"Transición de estado {Estado} a {nuevoEstado} no permitida.");
        }

        Estado = nuevoEstado;
        ComentariosEstado = comentarios;
        FechaEstado = DateTimeOffset.UtcNow;
        Touch();
    }
}
