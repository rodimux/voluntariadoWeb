using System;
using Volun.Core.Entities;
using Xunit;

namespace Volun.Tests.Domain;

public class VoluntarioTests
{
    [Fact]
    public void Anonimizar_ShouldClearSensitiveData()
    {
        var voluntario = Voluntario.Create(
            "Ana",
            "Gomez",
            "ana@example.com",
            DateTimeOffset.UtcNow.AddYears(-29),
            true,
            DateTimeOffset.UtcNow,
            telefono: "600000000",
            direccion: "Calle 1",
            provincia: "Madrid",
            pais: "Espana",
            disponibilidad: "Fines de semana");

        voluntario.EstablecerHabilidades(new[] { "Logistica" });
        voluntario.EstablecerPreferencias(new[] { "Cocina" });

        voluntario.Anonimizar();

        Assert.False(voluntario.EstaActivo);
        Assert.Equal(DateTimeOffset.UnixEpoch, voluntario.FechaNacimiento);
        Assert.Null(voluntario.Telefono);
        Assert.Null(voluntario.DniNie);
        Assert.Null(voluntario.Direccion);
        Assert.Null(voluntario.Provincia);
        Assert.Null(voluntario.Pais);
        Assert.Null(voluntario.Disponibilidad);
        Assert.StartsWith("anon-", voluntario.Email);
        Assert.Equal("Anonimo", voluntario.Nombre);
        Assert.Empty(voluntario.Preferencias);
        Assert.Empty(voluntario.Habilidades);
        Assert.False(voluntario.ConsentimientoRgpd);
    }
}
