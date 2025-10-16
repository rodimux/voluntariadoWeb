using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volun.Core.Entities;

namespace Volun.Infrastructure.Configurations;

public class InscripcionConfiguration : IEntityTypeConfiguration<Inscripcion>
{
    public void Configure(EntityTypeBuilder<Inscripcion> builder)
    {
        builder.ToTable("Inscripciones");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Estado)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(i => i.FechaSolicitud)
            .HasColumnType("datetimeoffset");

        builder.Property(i => i.FechaEstado)
            .HasColumnType("datetimeoffset");

        builder.Property(i => i.Notas)
            .HasMaxLength(1024);

        builder.Property(i => i.ComentariosEstado)
            .HasMaxLength(1024);

        builder.Property(i => i.QrToken)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(i => new { i.VoluntarioId, i.AccionId })
            .IsUnique();

        builder.HasMany(i => i.Asistencias)
            .WithOne(a => a.Inscripcion!)
            .HasForeignKey(a => a.InscripcionId);
    }
}
