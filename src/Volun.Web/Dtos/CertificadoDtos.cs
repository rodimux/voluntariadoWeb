namespace Volun.Web.Dtos;

public sealed record CreateCertificadoRequest(
    Guid VoluntarioId,
    Guid AccionId,
    decimal Horas);

public sealed record CertificadoResponse(
    Guid Id,
    Guid VoluntarioId,
    Guid AccionId,
    decimal Horas,
    DateTimeOffset EmitidoEn,
    string CodigoVerificacion,
    string? UrlPublica);
