# Volun – Gestor de Voluntariado

Solución ASP.NET Core 8 con arquitectura en capas para gestionar voluntarios, acciones, inscripciones, asistencia y certificados según el documento funcional `docs/funcional_gestor_voluntarios.md`.

## Estructura de la solución

```
.
├── docs/funcional_gestor_voluntarios.md
├── src/
│   ├── Volun.Core/                # Dominio: entidades, enums, contratos, servicios
│   ├── Volun.Infrastructure/      # EF Core, contexto, identity, seed, repositorios
│   ├── Volun.Notifications/       # Servicio de correo (MailKit) + plantillas
│   └── Volun.Web/                 # API v1, Razor Pages de ejemplo, autenticación, i18n
├── tests/
│   └── Volun.Tests/               # Pruebas unitarias e integradas (xUnit + WebApplicationFactory)
├── Directory.Build.props          # Configuración común (nullable, analyzers)
├── NuGet.config                   # Fuente nuget.org explícita
└── Volun.sln
```

## Puesta en marcha

1. **Restaurar y compilar**
   ```bash
   dotnet restore
   dotnet build
   ```
2. **Configurar la cadena de conexión**
   - Edita `src/Volun.Web/appsettings.Development.json` → `ConnectionStrings:DefaultConnection`.
3. **Ejecutar la migración inicial**
   ```bash
   dotnet ef migrations add InitialCreate -p src/Volun.Infrastructure -s src/Volun.Web
   dotnet ef database update -p src/Volun.Infrastructure -s src/Volun.Web
   ```
4. **Levantar la API**
   ```bash
   dotnet run --project src/Volun.Web
   ```
   - Swagger/JSON: `https://localhost:5001/swagger`
   - Razor Pages de ejemplo: `/Acciones`, `/Panel`, `/Perfil`
5. **Ejecutar pruebas**
   ```bash
   dotnet test
   ```

> La inicialización crea usuarios semilla:  
> - Admin → `admin@volun.local` / `ChangeThis!123`  
> - Coordinador → `coordinador@volun.local` / `ChangeThis!123`

## Endpoints y características clave

- API REST `api/v1/...` con Minimal APIs, `ProblemDetails`, validaciones FluentValidation y políticas por rol.
- Módulos disponibles V1:
  - `Acciones` + `Turnos`: CRUD básico, publicación, gestión de cupos.
  - `Voluntarios`: alta pública, actualización, soft delete (suspensión).
  - `Inscripciones`: creación con control de cupo/lista de espera, cambio de estado, check-in/out.
  - `Certificados` públicos: verificación por código.
  - `Reportes` y `Export` iniciales (CSV voluntarios).
- Integración con Identity + roles (Admin, Coordinador, Voluntario, Invitado).
- Localización ES/EN (`Localization/Shared.*.resx`).
- Servicio de correo (`EmailNotificationService`) configurable por `Notifications:Smtp`.
- Generadores stub para QR y PDF (inyectados vía DI con TODO explícito).

## Próximos pasos sugeridos

- Completar endpoints restantes (dashboard avanzado, exportaciones XLSX, certificados PDF reales).
- Sustituir stubs de QR/QuestPDF por implementaciones reales.
- Añadir autorizadores finos (coordinador sólo acciones propias) y pruebas adicionales.
- Implementar notificaciones con plantillas reales y orquestación de correos.

