using System.ComponentModel.DataAnnotations;
using Volun.Core.Enums;

namespace Volun.Web.Dtos;

public sealed record InscripcionResponse(
    Guid Id,
    Guid VoluntarioId,
    Guid AccionId,
    Guid? TurnoId,
    EstadoInscripcion Estado,
    DateTimeOffset FechaSolicitud,
    DateTimeOffset FechaEstado,
    string? Notas,
    string? ComentariosEstado,
    string QrToken);

public sealed record CreateInscripcionRequest(
    [property: Required] Guid VoluntarioId,
    [property: Required] Guid AccionId,
    Guid? TurnoId,
    [property: MaxLength(1024)] string? Notas);

public sealed record UpdateEstadoInscripcionRequest(
    [property: Required] EstadoInscripcion Estado,
    [property: MaxLength(1024)] string? Comentarios);
