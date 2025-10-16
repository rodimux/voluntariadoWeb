namespace Volun.Core.Entities;

public class AuditoriaRegistro : BaseEntity
{
    private AuditoriaRegistro()
    {
        // Requerido por EF Core
    }

    private AuditoriaRegistro(
        string entidad,
        Guid entidadId,
        string accion,
        string usuario,
        string datos,
        DateTimeOffset fecha)
    {
        Entidad = entidad;
        EntidadId = entidadId;
        Accion = accion;
        Usuario = usuario;
        Datos = datos;
        Fecha = fecha;
    }

    public string Entidad { get; private set; } = null!;
    public Guid EntidadId { get; private set; }
    public string Accion { get; private set; } = null!;
    public string Usuario { get; private set; } = null!;
    public string Datos { get; private set; } = null!;
    public DateTimeOffset Fecha { get; private set; }

    public static AuditoriaRegistro Crear(string entidad, Guid entidadId, string accion, string usuario, string datos)
        => new(entidad, entidadId, accion, usuario, datos, DateTimeOffset.UtcNow);
}
