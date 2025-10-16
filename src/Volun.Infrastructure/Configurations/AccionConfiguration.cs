using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volun.Core.Entities;
using Volun.Core.Enums;

namespace Volun.Infrastructure.Configurations;

public class AccionConfiguration : IEntityTypeConfiguration<Accion>
{
    public void Configure(EntityTypeBuilder<Accion> builder)
    {
        builder.ToTable("Acciones");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Titulo)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.Descripcion)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.Organizador)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.Ubicacion)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.Categoria)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(a => a.Requisitos)
            .HasMaxLength(1024);

        builder.Property(a => a.Tipo)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(a => a.Visibilidad)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(a => a.Estado)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(a => a.FechaInicio)
            .HasColumnType("datetimeoffset");

        builder.Property(a => a.FechaFin)
            .HasColumnType("datetimeoffset");

        builder.OwnsOne(a => a.GeoLocation, geo =>
        {
            geo.Property(g => g.Latitude)
                .HasColumnName("Latitud")
                .HasColumnType("float");
            geo.Property(g => g.Longitude)
                .HasColumnName("Longitud")
                .HasColumnType("float");
        });

        builder.HasMany(a => a.Turnos)
            .WithOne(t => t.Accion!)
            .HasForeignKey(t => t.AccionId);

        builder.HasMany(a => a.Inscripciones)
            .WithOne(i => i.Accion!)
            .HasForeignKey(i => i.AccionId);

        builder.Navigation(a => a.Turnos)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(a => a.Inscripciones)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
