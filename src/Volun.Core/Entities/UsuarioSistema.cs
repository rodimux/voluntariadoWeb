using Volun.Core.Enums;

namespace Volun.Core.Entities;

public class UsuarioSistema : BaseEntity
{
    private UsuarioSistema()
    {
        // Requerido por EF Core
    }

    private UsuarioSistema(string email, RolSistema rol, Guid? voluntarioId)
    {
        Email = email;
        Rol = rol;
        VoluntarioId = voluntarioId;
    }

    public string Email { get; private set; } = null!;
    public RolSistema Rol { get; private set; }
    public Guid? VoluntarioId { get; private set; }
    public bool EstaActivo { get; private set; } = true;

    public Voluntario? Voluntario { get; private set; }

    public static UsuarioSistema Create(string email, RolSistema rol, Guid? voluntarioId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("El email es obligatorio.", nameof(email));
        }

        return new UsuarioSistema(email.Trim().ToLowerInvariant(), rol, voluntarioId);
    }

    public void CambiarRol(RolSistema rol)
    {
        Rol = rol;
        Touch();
    }

    public void Desactivar()
    {
        EstaActivo = false;
        Touch();
    }

    public void Reactivar()
    {
        EstaActivo = true;
        Touch();
    }
}
