using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volun.Core.Entities;

namespace Volun.Infrastructure.Configurations;

public class AuditoriaRegistroConfiguration : IEntityTypeConfiguration<AuditoriaRegistro>
{
    public void Configure(EntityTypeBuilder<AuditoriaRegistro> builder)
    {
        builder.ToTable("Auditoria");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Entidad)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(a => a.Accion)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(a => a.Usuario)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(a => a.Datos)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.Fecha)
            .HasColumnType("datetimeoffset");
    }
}
