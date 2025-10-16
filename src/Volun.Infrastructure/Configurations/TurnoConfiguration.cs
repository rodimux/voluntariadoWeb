using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volun.Core.Entities;

namespace Volun.Infrastructure.Configurations;

public class TurnoConfiguration : IEntityTypeConfiguration<Turno>
{
    public void Configure(EntityTypeBuilder<Turno> builder)
    {
        builder.ToTable("Turnos");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Titulo)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.FechaInicio)
            .HasColumnType("datetimeoffset");

        builder.Property(t => t.FechaFin)
            .HasColumnType("datetimeoffset");

        builder.Property(t => t.Cupo)
            .IsRequired();

        builder.Property(t => t.Notas)
            .HasMaxLength(512);
    }
}
