using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Volun.Core.Enums;
using Volun.Core.ValueObjects;

namespace Volun.Core.Entities;

public class Accion : BaseEntity
{
    private readonly List<Turno> _turnos = [];
    private readonly List<Inscripcion> _inscripciones = [];

    private Accion()
    {
        // Requerido por EF Core
    }

    private Accion(
        string titulo,
        string descripcion,
        string organizador,
        string ubicacion,
        TipoAccion tipo,
        string categoria,
        DateTimeOffset fechaInicio,
        DateTimeOffset fechaFin,
        int cupoMaximo,
        Visibilidad visibilidad,
        bool turnosHabilitados,
        Guid? coordinadorId,
        GeoLocation? geoLocation = null,
        string? requisitos = null)
    {
        Titulo = titulo;
        Descripcion = descripcion;
        Organizador = organizador;
        Ubicacion = ubicacion;
        Tipo = tipo;
        Categoria = categoria;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        CupoMaximo = cupoMaximo;
        Visibilidad = visibilidad;
        TurnosHabilitados = turnosHabilitados;
        CoordinadorId = coordinadorId;
        GeoLocation = geoLocation;
        Requisitos = requisitos;
        Estado = EstadoAccion.Borrador;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string Titulo { get; private set; } = null!;
    public string Descripcion { get; private set; } = null!;
    public string Organizador { get; private set; } = null!;
    public string Ubicacion { get; private set; } = null!;
    public GeoLocation? GeoLocation { get; private set; }
    public TipoAccion Tipo { get; private set; }
    public string Categoria { get; private set; } = null!;
    public string? Requisitos { get; private set; }
    public int CupoMaximo { get; private set; }
    public Visibilidad Visibilidad { get; private set; }
    public DateTimeOffset FechaInicio { get; private set; }
    public DateTimeOffset FechaFin { get; private set; }
    public bool TurnosHabilitados { get; private set; }
    public EstadoAccion Estado { get; private set; }
    public Guid? CoordinadorId { get; private set; }

    public IReadOnlyCollection<Turno> Turnos => _turnos.AsReadOnly();
    public IReadOnlyCollection<Inscripcion> Inscripciones => _inscripciones.AsReadOnly();

    public static Accion Create(
        string titulo,
        string descripcion,
        string organizador,
        string ubicacion,
        TipoAccion tipo,
        string categoria,
        DateTimeOffset fechaInicio,
        DateTimeOffset fechaFin,
        int cupoMaximo,
        Visibilidad visibilidad,
        bool turnosHabilitados,
        Guid? coordinadorId,
        GeoLocation? geoLocation = null,
        string? requisitos = null)
    {
        if (fechaFin < fechaInicio)
        {
            throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio.");
        }

        if (cupoMaximo < 0)
        {
            throw new ArgumentException("El cupo mÃ¡ximo debe ser mayor o igual a 0.");
        }

        return new Accion(
            titulo,
            descripcion,
            organizador,
            ubicacion,
            tipo,
            categoria,
            fechaInicio,
            fechaFin,
            cupoMaximo,
            visibilidad,
            turnosHabilitados,
            coordinadorId,
            geoLocation,
            requisitos);
    }

    public void Publicar()
    {
        if (Estado != EstadoAccion.Borrador)
        {
            throw new InvalidOperationException("Solo las acciones en borrador pueden publicarse.");
        }

        Estado = EstadoAccion.Publicada;
        Touch();
    }

    public void Cerrar()
    {
        Estado = EstadoAccion.Cerrada;
        Touch();
    }

    public void ActualizarDetalle(
        string titulo,
        string descripcion,
        string ubicacion,
        DateTimeOffset fechaInicio,
        DateTimeOffset fechaFin,
        int cupoMaximo,
        Visibilidad visibilidad,
        string categoria,
        string? requisitos,
        GeoLocation? geoLocation)
    {
        if (fechaFin < fechaInicio)
        {
            throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio.");
        }

        Titulo = titulo;
        Descripcion = descripcion;
        Ubicacion = ubicacion;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        CupoMaximo = cupoMaximo;
        Visibilidad = visibilidad;
        Categoria = categoria;
        Requisitos = requisitos;
        GeoLocation = geoLocation;
        Touch();
    }

    public Turno AgregarTurno(
        string titulo,
        DateTimeOffset fechaInicio,
        DateTimeOffset fechaFin,
        int cupo,
        string? notas = null)
    {
        if (!TurnosHabilitados)
        {
            throw new InvalidOperationException("Los turnos no estÃ¡n habilitados para esta acciÃ³n.");
        }

        var turno = Turno.Create(Id, titulo, fechaInicio, fechaFin, cupo, notas);
        _turnos.Add(turno);
        Touch();
        return turno;
    }

    public bool TieneCupoDisponible(int solicitudesActuales)
        => CupoMaximo == 0 || solicitudesActuales < CupoMaximo;

    public int CupoDisponible(int solicitudesActuales)
        => CupoMaximo == 0 ? int.MaxValue : Math.Max(0, CupoMaximo - solicitudesActuales);
}
