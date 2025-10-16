using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volun.Core.Entities;

namespace Volun.Infrastructure.Configurations;

public class CertificadoConfiguration : IEntityTypeConfiguration<Certificado>
{
    public void Configure(EntityTypeBuilder<Certificado> builder)
    {
        builder.ToTable("Certificados");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Horas)
            .HasColumnType("decimal(6,2)");

        builder.Property(c => c.EmitidoEn)
            .HasColumnType("datetimeoffset");

        builder.Property(c => c.CodigoVerificacion)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(c => c.UrlPublica)
            .HasMaxLength(512);

        builder.HasIndex(c => c.CodigoVerificacion)
            .IsUnique();
    }
}
