using Microsoft.AspNetCore.Identity;

namespace Volun.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid? VoluntarioId { get; set; }
    public bool IsActive { get; set; } = true;
}
