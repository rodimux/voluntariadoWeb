using Volun.Core.Entities;
using Volun.Web.Dtos;

namespace Volun.Web.Mappings;

public static class MappingExtensions
{
    public static AccionResponse ToResponse(this Accion accion, int cupoDisponible)
        => new(
            accion.Id,
            accion.Titulo,
            accion.Descripcion,
            accion.Organizador,
            accion.Ubicacion,
            accion.Tipo,
            accion.Categoria,
            accion.Requisitos,
            accion.CupoMaximo,
            accion.Visibilidad,
            accion.FechaInicio,
            accion.FechaFin,
            accion.TurnosHabilitados,
            accion.Estado,
            accion.CoordinadorId,
            accion.GeoLocation?.Latitude,
            accion.GeoLocation?.Longitude,
            accion.Turnos.Select(t => new TurnoResponse(t.Id, t.Titulo, t.FechaInicio, t.FechaFin, t.Cupo, t.Notas)).ToList(),
            cupoDisponible);

    public static VoluntarioResponse ToResponse(this Voluntario voluntario)
        => new(
            voluntario.Id,
            voluntario.Nombre,
            voluntario.Apellidos,
            voluntario.Email,
            voluntario.Telefono,
            voluntario.FechaNacimiento,
            voluntario.DniNie,
            voluntario.Direccion,
            voluntario.Provincia,
            voluntario.Pais,
            voluntario.Disponibilidad,
            voluntario.EstaActivo,
            voluntario.Preferencias,
            voluntario.Habilidades,
            voluntario.FechaAlta);

    public static InscripcionResponse ToResponse(this Inscripcion inscripcion)
        => new(
            inscripcion.Id,
            inscripcion.VoluntarioId,
            inscripcion.AccionId,
            inscripcion.TurnoId,
            inscripcion.Estado,
            inscripcion.FechaSolicitud,
            inscripcion.FechaEstado,
            inscripcion.Notas,
            inscripcion.ComentariosEstado,
            inscripcion.QrToken);

    public static AsistenciaResponse ToResponse(this Asistencia asistencia)
        => new(
            asistencia.Id,
            asistencia.InscripcionId,
            asistencia.CheckIn,
            asistencia.CheckOut,
            asistencia.HorasComputadas,
            asistencia.Metodo,
            asistencia.Comentarios);

    public static CertificadoResponse ToResponse(this Certificado certificado)
        => new(
            certificado.Id,
            certificado.VoluntarioId,
            certificado.AccionId,
            certificado.Horas,
            certificado.EmitidoEn,
            certificado.CodigoVerificacion,
            certificado.UrlPublica);
}
