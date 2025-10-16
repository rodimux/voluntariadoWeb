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
using Serilog;
using System.Globalization;
using System.Security.Claims;
using Volun.Core.Entities;
using Volun.Core.Repositories;
using Volun.Core.Services;
using Volun.Infrastructure.Identity;
using Volun.Infrastructure.Persistence;
using Volun.Infrastructure.Persistence.Repositories;
using Volun.Infrastructure.Seed;
using Volun.Notifications;
using Volun.Web.Endpoints;
using Volun.Web.Endpoints.Admin;
using Volun.Web.Endpoints.Exports;
using Volun.Web.Endpoints.Public;

var builder = WebApplication.CreateBuilder(args);

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

    builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
            options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
        })
        .AddIdentityCookies();

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireAdminOrCoordinador", policy =>
            policy.RequireRole("Admin", "Coordinador"));
    });
}
else
{
    builder.Services.AddAuthentication("AllowAll")
        .AddScheme<AuthenticationSchemeOptions, AllowAllAuthenticationHandler>("AllowAll", _ => { });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireAdminOrCoordinador", policy =>
            policy.RequireAssertion(_ => true));
    });

    builder.Services.AddSingleton<IPolicyEvaluator, AllowAllPolicyEvaluator>();
}

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Notifications:Smtp"));
builder.Services.AddScoped<INotificationService, EmailNotificationService>();
builder.Services.AddScoped<IVoluntarioRepository, VoluntarioRepository>();
builder.Services.AddScoped<IAccionRepository, AccionRepository>();
builder.Services.AddScoped<IInscripcionRepository, InscripcionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IQrCodeGenerator, StubQrCodeGenerator>();
builder.Services.AddScoped<ICertificateService, StubCertificateService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<VolunDbContext>();
    var configuration = services.GetRequiredService<IConfiguration>();
    var useInMemoryDatabase = configuration.GetValue<bool>("UseInMemoryDatabase");

    if (useInMemoryDatabase)
    {
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        await db.Database.MigrateAsync();
    }

    if (!disableAuthentication)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeed");
        await DbSeed.InitializeAsync(roleManager, userManager, logger);
    }
}

var supportedCultures = new[] { new CultureInfo("es-ES"), new CultureInfo("en-US") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es-ES"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

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
app.MapCertificadosPublicEndpoints();
app.MapReportesEndpoints();
app.MapExportEndpoints();

app.Run();

public class StubQrCodeGenerator : IQrCodeGenerator
{
    public byte[] GenerateQr(string payload)
    {
        // TODO: Implement real QR generation with QRCoder
        return System.Text.Encoding.UTF8.GetBytes($"QR:{payload}");
    }
}

public class StubCertificateService : ICertificateService
{
    public Task<byte[]> GenerateCertificatePdfAsync(Certificado certificado, CancellationToken cancellationToken = default)
    {
        // TODO: Implement real PDF generation with QuestPDF
        var placeholder = $"Certificado {certificado.CodigoVerificacion} - Horas: {certificado.Horas}";
        return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(placeholder));
    }
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
        var principal = new ClaimsPrincipal(new ClaimsIdentity(Scheme.Name));
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class AllowAllPolicyEvaluator : IPolicyEvaluator
{
    public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity("AllowAll"));
        var ticket = new AuthenticationTicket(principal, "AllowAll");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    public Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object? resource)
        => Task.FromResult(PolicyAuthorizationResult.Success());
}

public partial class Program { }
