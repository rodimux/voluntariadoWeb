using Volun.Core.Entities;
using Volun.Core.Enums;
using Volun.Core.ValueObjects;
using Xunit;

namespace Volun.Tests.Domain;

public class AccionTests
{
    [Fact]
    public void CupoDisponible_ShouldReturnRemainingSlots()
    {
        var accion = Accion.Create(
            "Limpieza de playa",
            "Actividad medioambiental",
            "Fundación Mar",
            "Valencia",
            TipoAccion.Evento,
            "Medioambiente",
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(2),
            10,
            Visibilidad.Publica,
            false,
            Guid.NewGuid(),
            GeoLocation.From(39.47, -0.38));

        var disponible = accion.CupoDisponible(7);

        Assert.Equal(3, disponible);
    }

    [Fact]
    public void CupoDisponible_ShouldBeMaxValueWhenUnlimited()
    {
        var accion = Accion.Create(
            "Banco de alimentos",
            "Recolección",
            "Fundación",
            "Madrid",
            TipoAccion.Campaña,
            "Social",
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(2),
            0,
            Visibilidad.Publica,
            false,
            Guid.NewGuid(),
            null);

        var disponible = accion.CupoDisponible(1000);

        Assert.Equal(int.MaxValue, disponible);
    }
}
