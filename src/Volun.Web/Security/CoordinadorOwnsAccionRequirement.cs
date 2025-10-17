using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Volun.Core.Entities;
using Volun.Core.Enums;

namespace Volun.Web.Security;

public sealed class CoordinadorOwnsAccionRequirement : IAuthorizationRequirement
{
}

public sealed class CoordinadorOwnsAccionHandler
    : AuthorizationHandler<CoordinadorOwnsAccionRequirement, Accion>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CoordinadorOwnsAccionRequirement requirement,
        Accion? resource)
    {
        if (resource is null)
        {
            return Task.CompletedTask;
        }

        if (context.User.IsInRole(RolSistema.Admin.ToString()) || context.User.HasClaim(UserExtensions.TestingBypassClaimType, "true"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userId = context.User.GetUserId();
        if (userId is not null &&
            context.User.IsInRole(RolSistema.Coordinador.ToString()) &&
            resource.CoordinadorId == userId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
