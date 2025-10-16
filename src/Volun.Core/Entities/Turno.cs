namespace Volun.Core.Entities;

public class Turno : BaseEntity
{
    private Turno()
    {
        // Requerido por EF Core
    }

    private Turno(
        Guid accionId,
        string titulo,
        DateTimeOffset fechaInicio,
        DateTimeOffset fechaFin,
        int cupo,
        string? notas)
    {
        AccionId = accionId;
        Titulo = titulo;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        Cupo = cupo;
        Notas = notas;
    }

    public Guid AccionId { get; private set; }
    public string Titulo { get; private set; } = null!;
    public DateTimeOffset FechaInicio { get; private set; }
    public DateTimeOffset FechaFin { get; private set; }
    public int Cupo { get; private set; }
    public string? Notas { get; private set; }

    public Accion? Accion { get; private set; }
    public ICollection<Inscripcion> Inscripciones { get; private set; } = new List<Inscripcion>();

    public static Turno Create(
        Guid accionId,
        string titulo,
        DateTimeOffset fechaInicio,
        DateTimeOffset fechaFin,
        int cupo,
        string? notas = null)
    {
        if (fechaFin < fechaInicio)
        {
            throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio.");
        }

        if (cupo < 0)
        {
            throw new ArgumentException("El cupo debe ser mayor o igual que 0.");
        }

        return new Turno(accionId, titulo, fechaInicio, fechaFin, cupo, notas);
    }

    public void Actualizar(
        string titulo,
        DateTimeOffset fechaInicio,
        DateTimeOffset fechaFin,
        int cupo,
        string? notas)
    {
        if (fechaFin < fechaInicio)
        {
            throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio.");
        }

        if (cupo < 0)
        {
            throw new ArgumentException("El cupo debe ser mayor o igual que 0.");
        }

        Titulo = titulo;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        Cupo = cupo;
        Notas = notas;
        Touch();
    }
}
