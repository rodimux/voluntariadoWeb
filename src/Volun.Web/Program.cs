using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using Serilog;
using System.Globalization;
using System.Security.Claims;
using Volun.Core.Entities;
using Volun.Core.Enums;
using Volun.Core.Repositories;
using Volun.Core.Services;
using Volun.Infrastructure.Identity;
using Volun.Infrastructure.Persistence;
using Volun.Infrastructure.Persistence.Repositories;
using Volun.Infrastructure.Seed;
using Volun.Infrastructure.Services;
using Volun.Notifications;
using Volun.Web.Endpoints;
using Volun.Web.Endpoints.Admin;
using Volun.Web.Endpoints.Exports;
using Volun.Web.Endpoints.Public;
using Volun.Web.Services;
using Volun.Web.Security;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.AddLocalization(options => options.ResourcesPath = "Localization");
builder.Services.AddRazorPages()
    .AddViewLocalization();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Voluntariado API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Token JWT en el encabezado Authorization. Formato: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<VolunDbContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var useInMemoryDatabase = configuration.GetValue<bool>("UseInMemoryDatabase");

    if (useInMemoryDatabase)
    {
        options.UseInMemoryDatabase("VolunDbTests");
    }
    else
    {
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
            sql => sql.MigrationsAssembly(typeof(VolunDbContext).Assembly.FullName));
    }
});

var disableAuthentication = builder.Environment.IsEnvironment("Testing") || builder.Configuration.GetValue<bool>("DisableAuthentication");

if (!disableAuthentication)
{
    builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<VolunDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
            policy.RequireRole(AuthorizationPolicies.RoleName(RolSistema.Admin)));

        options.AddPolicy(AuthorizationPolicies.CoordinadorOnly, policy =>
            policy.RequireRole(AuthorizationPolicies.RoleName(RolSistema.Coordinador)));

        options.AddPolicy(AuthorizationPolicies.VoluntarioOnly, policy =>
            policy.RequireRole(AuthorizationPolicies.RoleName(RolSistema.Voluntario)));

        options.AddPolicy(AuthorizationPolicies.AdminOrCoordinador, policy =>
            policy.RequireRole(
                AuthorizationPolicies.RoleName(RolSistema.Admin),
                AuthorizationPolicies.RoleName(RolSistema.Coordinador)));
    });
}
else
{
    builder.Services.AddAuthentication("AllowAll")
        .AddScheme<AuthenticationSchemeOptions, AllowAllAuthenticationHandler>("AllowAll", _ => { });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
            policy.RequireAssertion(_ => true));
        options.AddPolicy(AuthorizationPolicies.CoordinadorOnly, policy =>
            policy.RequireAssertion(_ => true));
        options.AddPolicy(AuthorizationPolicies.VoluntarioOnly, policy =>
            policy.RequireAssertion(_ => true));
        options.AddPolicy(AuthorizationPolicies.AdminOrCoordinador, policy =>
            policy.RequireAssertion(_ => true));
    });

    builder.Services.AddSingleton<IPolicyEvaluator, AllowAllPolicyEvaluator>();
}

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Notifications:Smtp"));
builder.Services.AddScoped<INotificationService, EmailNotificationService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IVoluntarioRepository, VoluntarioRepository>();
builder.Services.AddScoped<IAccionRepository, AccionRepository>();
builder.Services.AddScoped<IInscripcionRepository, InscripcionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IQrCodeGenerator, QrCodeGeneratorService>();
builder.Services.AddScoped<ICertificateService, CertificatePdfService>();
builder.Services.AddSingleton<IAuthorizationHandler, CoordinadorOwnsAccionHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<VolunDbContext>();
    var configuration = services.GetRequiredService<IConfiguration>();
    var useInMemoryDatabase = configuration.GetValue<bool>("UseInMemoryDatabase");
    var seedSampleData = configuration.GetValue<bool>("SeedSampleData");
    var skipIdentitySeed = configuration.GetValue<bool>("SkipIdentitySeed");
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    var seedLogger = loggerFactory.CreateLogger("DbSeed");

    if (useInMemoryDatabase)
    {
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        await db.Database.MigrateAsync();
    }

    if (!disableAuthentication && !skipIdentitySeed)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        await DbSeed.InitializeAsync(roleManager, userManager, seedLogger);
    }

    if (seedSampleData)
    {
        await DevelopmentDataSeeder.SeedAsync(db, seedLogger);
    }
}

var supportedCultures = new[] { new CultureInfo("es-ES"), new CultureInfo("en-US") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es-ES"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

if (disableAuthentication)
{
    app.Use(async (context, next) =>
    {
        var identity = new ClaimsIdentity("Testing", ClaimTypes.Name, ClaimTypes.Role);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, TestingAuthConstants.UserId.ToString()));
        identity.AddClaim(new Claim(identity.RoleClaimType, RolSistema.Admin.ToString()));
        identity.AddClaim(new Claim(identity.RoleClaimType, RolSistema.Coordinador.ToString()));
        identity.AddClaim(new Claim(identity.RoleClaimType, RolSistema.Voluntario.ToString()));
        identity.AddClaim(new Claim(UserExtensions.VoluntarioIdClaimType, TestingAuthConstants.VoluntarioId.ToString()));
        identity.AddClaim(new Claim(UserExtensions.TestingBypassClaimType, "true"));
        context.User = new ClaimsPrincipal(identity);
        await next();
    });
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapAccionesEndpoints();
app.MapVoluntariosEndpoints();
app.MapInscripcionesEndpoints();
app.MapCertificadosEndpoints();
app.MapCertificadosPublicEndpoints();
app.MapReportesEndpoints();
app.MapExportEndpoints();

app.Run();

public static class TestingAuthConstants
{
    public static Guid UserId { get; set; } = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static Guid VoluntarioId { get; set; } = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
}

public class AllowAllAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public AllowAllAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(Scheme.Name, ClaimTypes.Name, ClaimTypes.Role);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, TestingAuthConstants.UserId.ToString()));
        identity.AddClaim(new Claim(identity.RoleClaimType, RolSistema.Admin.ToString()));
        identity.AddClaim(new Claim(identity.RoleClaimType, RolSistema.Coordinador.ToString()));
        identity.AddClaim(new Claim(identity.RoleClaimType, RolSistema.Voluntario.ToString()));
        identity.AddClaim(new Claim(UserExtensions.VoluntarioIdClaimType, TestingAuthConstants.VoluntarioId.ToString()));
        identity.AddClaim(new Claim(UserExtensions.TestingBypassClaimType, "true"));
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class AllowAllPolicyEvaluator : IPolicyEvaluator
{
    public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        var identity = new ClaimsIdentity("AllowAll", ClaimTypes.Name, ClaimTypes.Role);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, TestingAuthConstants.UserId.ToString()));
        identity.AddClaim(new Claim(identity.RoleClaimType, RolSistema.Admin.ToString()));
        identity.AddClaim(new Claim(identity.RoleClaimType, RolSistema.Coordinador.ToString()));
        identity.AddClaim(new Claim(identity.RoleClaimType, RolSistema.Voluntario.ToString()));
        identity.AddClaim(new Claim(UserExtensions.VoluntarioIdClaimType, TestingAuthConstants.VoluntarioId.ToString()));
        identity.AddClaim(new Claim(UserExtensions.TestingBypassClaimType, "true"));
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "AllowAll");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    public Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object? resource)
        => Task.FromResult(PolicyAuthorizationResult.Success());
}

public partial class Program { }
