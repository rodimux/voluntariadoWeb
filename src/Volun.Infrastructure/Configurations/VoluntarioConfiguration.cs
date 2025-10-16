using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volun.Core.Entities;

namespace Volun.Infrastructure.Configurations;

public class VoluntarioConfiguration : IEntityTypeConfiguration<Voluntario>
{
    public void Configure(EntityTypeBuilder<Voluntario> builder)
    {
        builder.ToTable("Voluntarios");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(v => v.Email)
            .IsUnique();

        builder.Property(v => v.Nombre)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(v => v.Apellidos)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(v => v.Telefono)
            .HasMaxLength(32);

        builder.Property(v => v.DniNie)
            .HasMaxLength(32);

        builder.Property(v => v.Direccion)
            .HasMaxLength(256);

        builder.Property(v => v.Provincia)
            .HasMaxLength(128);

        builder.Property(v => v.Pais)
            .HasMaxLength(128);

        builder.Property(v => v.Disponibilidad)
            .HasMaxLength(512);

        builder.Property(v => v.EstaActivo)
            .HasDefaultValue(true);

        builder.Property(v => v.FechaAlta)
            .HasColumnType("datetimeoffset");

        builder.Property(v => v.FechaActualizacion)
            .HasColumnType("datetimeoffset");

        builder.Property(v => v.ConsentimientoRgpdFecha)
            .HasColumnType("datetimeoffset");

        builder.Property<HashSet<string>>("_preferencias")
            .HasConversion(
                set => JsonSerializer.Serialize(set, (JsonSerializerOptions?)null),
                json => json == null
                    ? new HashSet<string>()
                    : JsonSerializer.Deserialize<HashSet<string>>(json, (JsonSerializerOptions?)null) ?? new HashSet<string>())
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("Preferencias")
            .HasColumnType("nvarchar(max)");

        builder.Property<HashSet<string>>("_habilidades")
            .HasConversion(
                set => JsonSerializer.Serialize(set, (JsonSerializerOptions?)null),
                json => json == null
                    ? new HashSet<string>()
                    : JsonSerializer.Deserialize<HashSet<string>>(json, (JsonSerializerOptions?)null) ?? new HashSet<string>())
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("Habilidades")
            .HasColumnType("nvarchar(max)");

        builder.HasMany(v => v.Inscripciones)
            .WithOne(i => i.Voluntario!)
            .HasForeignKey(i => i.VoluntarioId);

        builder.Navigation(v => v.Inscripciones)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
