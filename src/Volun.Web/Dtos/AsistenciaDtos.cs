using System.ComponentModel.DataAnnotations;
using Volun.Core.Enums;

namespace Volun.Web.Dtos;

public sealed record CheckInRequest(
    [property: Required] Guid InscripcionId,
    MetodoAsistencia Metodo,
    string? Comentarios);

public sealed record CheckOutRequest(
    [property: Required] Guid InscripcionId,
    [property: Required] DateTimeOffset CheckOut,
    string? Comentarios);

public sealed record AsistenciaResponse(
    Guid Id,
    Guid InscripcionId,
    DateTimeOffset CheckIn,
    DateTimeOffset? CheckOut,
    decimal? HorasComputadas,
    MetodoAsistencia Metodo,
    string? Comentarios);
