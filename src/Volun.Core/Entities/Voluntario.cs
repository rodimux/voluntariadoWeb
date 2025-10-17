using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;

namespace Volun.Core.Entities;

public class Voluntario : BaseEntity
{
    private readonly HashSet<string> _habilidades = [];
    private readonly HashSet<string> _preferencias = [];
    private readonly List<Inscripcion> _inscripciones = [];

    private Voluntario()
    {
        // Requerido por EF Core
    }

    private Voluntario(
        string nombre,
        string apellidos,
        string email,
        DateTimeOffset fechaNacimiento,
        bool consentimientoRgpd,
        DateTimeOffset consentimientoRgpdFecha,
        string? telefono = null,
        string? dniNie = null,
        string? direccion = null,
        string? provincia = null,
        string? pais = null,
        string? disponibilidad = null)
    {
        Nombre = nombre;
        Apellidos = apellidos;
        Email = email;
        FechaNacimiento = fechaNacimiento;
        Telefono = telefono;
        DniNie = dniNie;
        Direccion = direccion;
        Provincia = provincia;
        Pais = pais;
        Disponibilidad = disponibilidad;
        ConsentimientoRgpd = consentimientoRgpd;
        ConsentimientoRgpdFecha = consentimientoRgpdFecha;
        EstaActivo = true;
        FechaAlta = DateTimeOffset.UtcNow;
        CreatedAt = FechaAlta;
    }

    public string Nombre { get; private set; } = null!;
    public string Apellidos { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? Telefono { get; private set; }
    public DateTimeOffset FechaNacimiento { get; private set; }
    public string? DniNie { get; private set; }
    public string? Direccion { get; private set; }
    public string? Provincia { get; private set; }
    public string? Pais { get; private set; }
    public string? Disponibilidad { get; private set; }
    public bool ConsentimientoRgpd { get; private set; }
    public DateTimeOffset ConsentimientoRgpdFecha { get; private set; }
    public bool EstaActivo { get; private set; }
    public DateTimeOffset FechaAlta { get; private set; }
    public DateTimeOffset? FechaActualizacion { get; private set; }

    public IReadOnlyCollection<string> Preferencias => _preferencias.ToList().AsReadOnly();
    public IReadOnlyCollection<string> Habilidades => _habilidades.ToList().AsReadOnly();
    public IReadOnlyCollection<Inscripcion> Inscripciones => _inscripciones.AsReadOnly();

    public static Voluntario Create(
        string nombre,
        string apellidos,
        string email,
        DateTimeOffset fechaNacimiento,
        bool consentimientoRgpd,
        DateTimeOffset consentimientoRgpdFecha,
        string? telefono = null,
        string? dniNie = null,
        string? direccion = null,
        string? provincia = null,
        string? pais = null,
        string? disponibilidad = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        }

        if (string.IsNullOrWhiteSpace(apellidos))
        {
            throw new ArgumentException("Los apellidos son obligatorios.", nameof(apellidos));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("El email es obligatorio.", nameof(email));
        }

        return new Voluntario(
            nombre,
            apellidos,
            email.Trim().ToLowerInvariant(),
            fechaNacimiento,
            consentimientoRgpd,
            consentimientoRgpdFecha,
            telefono,
            dniNie,
            direccion,
            provincia,
            pais,
            disponibilidad);
    }

    public void ActualizarPerfil(
        string nombre,
        string apellidos,
        string? telefono,
        string? direccion,
        string? provincia,
        string? pais,
        string? disponibilidad)
    {
        Nombre = nombre;
        Apellidos = apellidos;
        Telefono = telefono;
        Direccion = direccion;
        Provincia = provincia;
        Pais = pais;
        Disponibilidad = disponibilidad;
        FechaActualizacion = DateTimeOffset.UtcNow;
        Touch();
    }

    public void EstablecerPreferencias(IEnumerable<string> preferencias)
    {
        _preferencias.Clear();
        foreach (var preferencia in preferencias.Where(p => !string.IsNullOrWhiteSpace(p)))
        {
            _preferencias.Add(preferencia.Trim());
        }
        FechaActualizacion = DateTimeOffset.UtcNow;
        Touch();
    }

    public void EstablecerHabilidades(IEnumerable<string> habilidades)
    {
        _habilidades.Clear();
        foreach (var habilidad in habilidades.Where(p => !string.IsNullOrWhiteSpace(p)))
        {
            _habilidades.Add(habilidad.Trim());
        }
        FechaActualizacion = DateTimeOffset.UtcNow;
        Touch();
    }

    public void Suspender()
    {
        EstaActivo = false;
        FechaActualizacion = DateTimeOffset.UtcNow;
        Touch();
    }

    public void Reactivar()
    {
        EstaActivo = true;
        FechaActualizacion = DateTimeOffset.UtcNow;
        Touch();
    }

    public void Anonimizar()
    {
        var suffix = Id.ToString("N")[..8];
        Nombre = "Anonimo";
        Apellidos = $"Voluntario-{suffix}";
        Email = $"anon-{suffix}@anon.volun";
        Telefono = null;
        DniNie = null;
        Direccion = null;
        Provincia = null;
        Pais = null;
        Disponibilidad = null;
        FechaNacimiento = DateTimeOffset.UnixEpoch;
        ConsentimientoRgpd = false;
        ConsentimientoRgpdFecha = DateTimeOffset.UtcNow;
        EstaActivo = false;

        _habilidades.Clear();
        _preferencias.Clear();

        FechaActualizacion = DateTimeOffset.UtcNow;
        Touch();
    }

    public override string ToString() => JsonSerializer.Serialize(new
    {
        Id,
        NombreCompleto = $"{Nombre} {Apellidos}",
        Email,
        EstaActivo
    });
}
