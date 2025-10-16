using Microsoft.AspNetCore.Identity;
using System.Linq;
using Microsoft.Extensions.Logging;
using Volun.Core.Enums;
using Volun.Infrastructure.Identity;

namespace Volun.Infrastructure.Seed;

public static class DbSeed
{
    public static async Task InitializeAsync(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        await EnsureRolesAsync(roleManager, logger, cancellationToken);
        await EnsureAdminUserAsync(userManager, logger, cancellationToken);
        await EnsureCoordinadorUserAsync(userManager, logger, cancellationToken);
    }

    private static async Task EnsureRolesAsync(
        RoleManager<ApplicationRole> roleManager,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        foreach (var roleName in Enum.GetNames<RolSistema>())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new ApplicationRole(roleName));
            if (!result.Succeeded)
            {
                logger.LogWarning("No se pudo crear el rol {Role}: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private static async Task EnsureAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        const string email = "admin@volun.local";
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
        {
            if (!await userManager.IsInRoleAsync(user, RolSistema.Admin.ToString()))
            {
                await userManager.AddToRoleAsync(user, RolSistema.Admin.ToString());
            }
            return;
        }

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, "ChangeThis!123");
        if (!result.Succeeded)
        {
            logger.LogWarning("No se pudo crear el usuario admin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(user, RolSistema.Admin.ToString());
    }

    private static async Task EnsureCoordinadorUserAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        const string email = "coordinador@volun.local";
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
        {
            if (!await userManager.IsInRoleAsync(user, RolSistema.Coordinador.ToString()))
            {
                await userManager.AddToRoleAsync(user, RolSistema.Coordinador.ToString());
            }
            return;
        }

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, "ChangeThis!123");
        if (!result.Succeeded)
        {
            logger.LogWarning("No se pudo crear el usuario coordinador: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(user, RolSistema.Coordinador.ToString());
    }
}
