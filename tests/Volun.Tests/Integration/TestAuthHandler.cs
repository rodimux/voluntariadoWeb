using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volun.Web.Security;

namespace Volun.Tests.Integration;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    public const string HeaderName = "X-Test-User";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var headerValues))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing test user header"));
        }

        var header = headerValues.ToString();
        if (string.IsNullOrWhiteSpace(header))
        {
            return Task.FromResult(AuthenticateResult.Fail("Empty test user header"));
        }

        var parts = header.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid test user header format"));
        }

        if (!Guid.TryParse(parts[0], out var userId))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid user id"));
        }

        var roles = parts[1]
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (parts.Length >= 3 && Guid.TryParse(parts[2], out var voluntarioId))
        {
            claims.Add(new Claim(UserExtensions.VoluntarioIdClaimType, voluntarioId.ToString()));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
