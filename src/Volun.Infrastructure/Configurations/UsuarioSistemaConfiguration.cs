using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volun.Core.Entities;

namespace Volun.Infrastructure.Configurations;

public class UsuarioSistemaConfiguration : IEntityTypeConfiguration<UsuarioSistema>
{
    public void Configure(EntityTypeBuilder<UsuarioSistema> builder)
    {
        builder.ToTable("UsuariosSistema");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.Rol)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(u => u.EstaActivo)
            .HasDefaultValue(true);

        builder.HasOne(u => u.Voluntario)
            .WithMany()
            .HasForeignKey(u => u.VoluntarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
