using System.ComponentModel.DataAnnotations;

namespace Volun.Web.Dtos;

public sealed record VoluntarioResponse(
    Guid Id,
    string Nombre,
    string Apellidos,
    string Email,
    string? Telefono,
    DateTimeOffset FechaNacimiento,
    string? DniNie,
    string? Direccion,
    string? Provincia,
    string? Pais,
    string? Disponibilidad,
    bool EstaActivo,
    IEnumerable<string> Preferencias,
    IEnumerable<string> Habilidades,
    DateTimeOffset FechaAlta);

public sealed record CreateVoluntarioRequest(
    [property: Required, MaxLength(128)] string Nombre,
    [property: Required, MaxLength(256)] string Apellidos,
    [property: Required, EmailAddress, MaxLength(256)] string Email,
    [property: Required] DateTimeOffset FechaNacimiento,
    [property: Required] bool ConsentimientoRgpd,
    [property: Phone] string? Telefono,
    [property: MaxLength(32)] string? DniNie,
    [property: MaxLength(256)] string? Direccion,
    [property: MaxLength(128)] string? Provincia,
    [property: MaxLength(128)] string? Pais,
    [property: MaxLength(512)] string? Disponibilidad,
    IEnumerable<string>? Preferencias,
    IEnumerable<string>? Habilidades);

public sealed record UpdateVoluntarioRequest(
    [property: Required, MaxLength(128)] string Nombre,
    [property: Required, MaxLength(256)] string Apellidos,
    [property: Phone] string? Telefono,
    [property: MaxLength(256)] string? Direccion,
    [property: MaxLength(128)] string? Provincia,
    [property: MaxLength(128)] string? Pais,
    [property: MaxLength(512)] string? Disponibilidad,
    IEnumerable<string>? Preferencias,
    IEnumerable<string>? Habilidades);
