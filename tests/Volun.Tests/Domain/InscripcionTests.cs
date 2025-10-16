using Volun.Core.Entities;
using Volun.Core.Enums;
using Xunit;

namespace Volun.Tests.Domain;

public class InscripcionTests
{
    [Fact]
    public void CambiarEstado_ShouldThrow_WhenTransitionNotAllowed()
    {
        var inscripcion = Inscripcion.Create(Guid.NewGuid(), Guid.NewGuid(), null);
        inscripcion.CambiarEstado(EstadoInscripcion.Aprobada);

        Assert.Throws<InvalidOperationException>(() => inscripcion.CambiarEstado(EstadoInscripcion.Pendiente));
    }

    [Fact]
    public void CambiarEstado_ShouldUpdate_WhenTransitionAllowed()
    {
        var inscripcion = Inscripcion.Create(Guid.NewGuid(), Guid.NewGuid(), null);
        inscripcion.CambiarEstado(EstadoInscripcion.Aprobada);

        inscripcion.CambiarEstado(EstadoInscripcion.Completada);

        Assert.Equal(EstadoInscripcion.Completada, inscripcion.Estado);
    }
}
