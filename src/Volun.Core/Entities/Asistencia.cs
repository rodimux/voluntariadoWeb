using Volun.Core.Enums;

namespace Volun.Core.Entities;

public class Asistencia : BaseEntity
{
    private Asistencia()
    {
        // Requerido por EF Core
    }

    private Asistencia(Guid inscripcionId, DateTimeOffset checkIn, MetodoAsistencia metodo, string? comentarios)
    {
        InscripcionId = inscripcionId;
        CheckIn = checkIn;
        Metodo = metodo;
        Comentarios = comentarios;
    }

    public Guid InscripcionId { get; private set; }
    public DateTimeOffset CheckIn { get; private set; }
    public DateTimeOffset? CheckOut { get; private set; }
    public decimal? HorasComputadas { get; private set; }
    public MetodoAsistencia Metodo { get; private set; }
    public string? Comentarios { get; private set; }

    public Inscripcion? Inscripcion { get; private set; }

    public static Asistencia Create(Guid inscripcionId, DateTimeOffset checkIn, MetodoAsistencia metodo, string? comentarios = null)
        => new(inscripcionId, checkIn, metodo, comentarios);

    public void RegistrarCheckOut(DateTimeOffset checkOut, string? comentarios = null)
    {
        if (checkOut < CheckIn)
        {
            throw new ArgumentException("La fecha de checkout no puede ser anterior al checkin.");
        }

        CheckOut = checkOut;
        var totalHoras = (checkOut - CheckIn).TotalHours;
        HorasComputadas = Math.Round((decimal)totalHoras, 2, MidpointRounding.AwayFromZero);
        Comentarios = comentarios ?? Comentarios;
        Touch();
    }
}
