using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volun.Core.Entities;
using Volun.Core.Enums;
using Volun.Infrastructure.Persistence;

namespace Volun.Infrastructure.Seed;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(VolunDbContext dbContext, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Voluntarios.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Sample data seeding skipped because records already exist.");
            return;
        }

        logger.LogInformation("Seeding sample development data...");

        var voluntarios = new[]
        {
            Voluntario.Create(
                nombre: "Laura",
                apellidos: "García Pérez",
                email: "laura.garcia@example.com",
                fechaNacimiento: new DateTimeOffset(1992, 05, 14, 0, 0, 0, TimeSpan.Zero),
                consentimientoRgpd: true,
                consentimientoRgpdFecha: DateTimeOffset.UtcNow.AddMonths(-6),
                telefono: "+34 600000001",
                provincia: "Madrid",
                pais: "España",
                disponibilidad: "Fines de semana"),
            Voluntario.Create(
                nombre: "Carlos",
                apellidos: "López Martín",
                email: "carlos.lopez@example.com",
                fechaNacimiento: new DateTimeOffset(1988, 11, 03, 0, 0, 0, TimeSpan.Zero),
                consentimientoRgpd: true,
                consentimientoRgpdFecha: DateTimeOffset.UtcNow.AddMonths(-4),
                telefono: "+34 600000002",
                provincia: "Barcelona",
                pais: "España",
                disponibilidad: "Tardes"),
            Voluntario.Create(
                nombre: "Ana",
                apellidos: "Ruiz Sánchez",
                email: "ana.ruiz@example.com",
                fechaNacimiento: new DateTimeOffset(1995, 02, 21, 0, 0, 0, TimeSpan.Zero),
                consentimientoRgpd: true,
                consentimientoRgpdFecha: DateTimeOffset.UtcNow.AddMonths(-2),
                telefono: "+34 600000003",
                provincia: "Valencia",
                pais: "España",
                disponibilidad: "Mañanas")
        };

        dbContext.Voluntarios.AddRange(voluntarios);
        await dbContext.SaveChangesAsync(cancellationToken);

        var accion = Accion.Create(
            titulo: "Recogida de alimentos",
            descripcion: "Campaña de recogida de alimentos para familias vulnerables.",
            organizador: "Fundación Local",
            ubicacion: "Madrid",
            tipo: TipoAccion.Evento,
            categoria: "Solidaridad",
            fechaInicio: DateTimeOffset.UtcNow.AddDays(-10),
            fechaFin: DateTimeOffset.UtcNow.AddDays(10),
            cupoMaximo: 50,
            visibilidad: Visibilidad.Publica,
            turnosHabilitados: true,
            coordinadorId: null,
            requisitos: "Ganas de ayudar",
            geoLocation: null);

        var turnoManana = accion.AgregarTurno(
            titulo: "Turno mañana",
            fechaInicio: DateTimeOffset.UtcNow.Date.AddHours(8),
            fechaFin: DateTimeOffset.UtcNow.Date.AddHours(12),
            cupo: 25,
            notas: "Montaje de puestos");

        var turnoTarde = accion.AgregarTurno(
            titulo: "Turno tarde",
            fechaInicio: DateTimeOffset.UtcNow.Date.AddHours(14),
            fechaFin: DateTimeOffset.UtcNow.Date.AddHours(18),
            cupo: 25,
            notas: "Distribución de alimentos");

        dbContext.Acciones.Add(accion);
        await dbContext.SaveChangesAsync(cancellationToken);

        var inscripcionLaura = Inscripcion.Create(voluntarios[0].Id, accion.Id, turnoManana.Id, "Disponible los sábados.");
        inscripcionLaura.CambiarEstado(EstadoInscripcion.Aprobada, "Confirmada por coordinador");

        var inscripcionCarlos = Inscripcion.Create(voluntarios[1].Id, accion.Id, turnoTarde.Id, "Puede apoyar con transporte.");
        inscripcionCarlos.CambiarEstado(EstadoInscripcion.Aprobada, "Asignado al turno de tarde");
        inscripcionCarlos.CambiarEstado(EstadoInscripcion.Completada, "Actividad realizada");

        var inscripcionAna = Inscripcion.Create(voluntarios[2].Id, accion.Id, turnoManana.Id, "Se incorpora la próxima semana.");
        inscripcionAna.CambiarEstado(EstadoInscripcion.ListaEspera, "En espera de disponibilidad");

        dbContext.Inscripciones.AddRange(inscripcionLaura, inscripcionCarlos, inscripcionAna);
        await dbContext.SaveChangesAsync(cancellationToken);

        var asistenciaCarlos = Asistencia.Create(
            inscripcionCarlos.Id,
            DateTimeOffset.UtcNow.AddDays(-1).AddHours(9),
            MetodoAsistencia.Manual,
            "Registrado en punto de control");
        asistenciaCarlos.RegistrarCheckOut(asistenciaCarlos.CheckIn.AddHours(3));

        dbContext.Asistencias.Add(asistenciaCarlos);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Sample development data seeded successfully.");
    }
}
