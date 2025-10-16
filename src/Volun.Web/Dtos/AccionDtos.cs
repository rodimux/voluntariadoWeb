using System.ComponentModel.DataAnnotations;
using Volun.Core.Enums;

namespace Volun.Web.Dtos;

public sealed record AccionResponse(
    Guid Id,
    string Titulo,
    string Descripcion,
    string Organizador,
    string Ubicacion,
    TipoAccion Tipo,
    string Categoria,
    string? Requisitos,
    int CupoMaximo,
    Visibilidad Visibilidad,
    DateTimeOffset FechaInicio,
    DateTimeOffset FechaFin,
    bool TurnosHabilitados,
    EstadoAccion Estado,
    Guid? CoordinadorId,
    double? Latitud,
    double? Longitud,
    IReadOnlyCollection<TurnoResponse> Turnos,
    int CupoDisponible);

public sealed record TurnoResponse(
    Guid Id,
    string Titulo,
    DateTimeOffset FechaInicio,
    DateTimeOffset FechaFin,
    int Cupo,
    string? Notas);

public sealed record CreateAccionRequest(
    [property: Required, MaxLength(256)] string Titulo,
    [property: Required] string Descripcion,
    [property: Required, MaxLength(256)] string Organizador,
    [property: Required, MaxLength(256)] string Ubicacion,
    [property: Required] TipoAccion Tipo,
    [property: Required, MaxLength(128)] string Categoria,
    [property: MaxLength(1024)] string? Requisitos,
    [property: Range(0, int.MaxValue)] int CupoMaximo,
    [property: Required] Visibilidad Visibilidad,
    [property: Required] DateTimeOffset FechaInicio,
    [property: Required] DateTimeOffset FechaFin,
    bool TurnosHabilitados,
    Guid? CoordinadorId,
    double? Latitud,
    double? Longitud);

public sealed record UpdateAccionRequest(
    [property: Required, MaxLength(256)] string Titulo,
    [property: Required] string Descripcion,
    [property: Required, MaxLength(256)] string Ubicacion,
    [property: Required] DateTimeOffset FechaInicio,
    [property: Required] DateTimeOffset FechaFin,
    [property: Range(0, int.MaxValue)] int CupoMaximo,
    [property: Required] Visibilidad Visibilidad,
    [property: Required, MaxLength(128)] string Categoria,
    [property: MaxLength(1024)] string? Requisitos,
    double? Latitud,
    double? Longitud);

public sealed record CreateTurnoRequest(
    [property: Required, MaxLength(256)] string Titulo,
    [property: Required] DateTimeOffset FechaInicio,
    [property: Required] DateTimeOffset FechaFin,
    [property: Range(0, int.MaxValue)] int Cupo,
    [property: MaxLength(512)] string? Notas);
