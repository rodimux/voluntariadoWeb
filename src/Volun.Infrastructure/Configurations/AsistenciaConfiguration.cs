using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volun.Core.Entities;

namespace Volun.Infrastructure.Configurations;

public class AsistenciaConfiguration : IEntityTypeConfiguration<Asistencia>
{
    public void Configure(EntityTypeBuilder<Asistencia> builder)
    {
        builder.ToTable("Asistencias");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.CheckIn)
            .HasColumnType("datetimeoffset");

        builder.Property(a => a.CheckOut)
            .HasColumnType("datetimeoffset");

        builder.Property(a => a.HorasComputadas)
            .HasColumnType("decimal(6,2)");

        builder.Property(a => a.Metodo)
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.Property(a => a.Comentarios)
            .HasMaxLength(512);
    }
}
