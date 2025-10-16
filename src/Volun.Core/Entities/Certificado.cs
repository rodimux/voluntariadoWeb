namespace Volun.Core.Entities;

public class Certificado : BaseEntity
{
    private Certificado()
    {
        // Requerido por EF Core
    }

    private Certificado(Guid voluntarioId, Guid accionId, decimal horas, DateTimeOffset emitidoEn, string codigoVerificacion)
    {
        VoluntarioId = voluntarioId;
        AccionId = accionId;
        Horas = horas;
        EmitidoEn = emitidoEn;
        CodigoVerificacion = codigoVerificacion;
    }

    public Guid VoluntarioId { get; private set; }
    public Guid AccionId { get; private set; }
    public decimal Horas { get; private set; }
    public DateTimeOffset EmitidoEn { get; private set; }
    public string CodigoVerificacion { get; private set; } = null!;
    public string? UrlPublica { get; private set; }

    public Voluntario? Voluntario { get; private set; }
    public Accion? Accion { get; private set; }

    public static Certificado Emitir(Guid voluntarioId, Guid accionId, decimal horas, string codigoVerificacion, string? urlPublica)
    {
        if (horas <= 0)
        {
            throw new ArgumentException("Las horas deben ser mayores a cero.", nameof(horas));
        }

        var certificado = new Certificado(voluntarioId, accionId, decimal.Round(horas, 2), DateTimeOffset.UtcNow, codigoVerificacion)
        {
            UrlPublica = urlPublica
        };

        return certificado;
    }
}
