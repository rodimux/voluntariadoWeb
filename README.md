# Volun – Gestor de Voluntariado

Solucion ASP.NET Core 8 en arquitectura en capas para gestionar voluntarios, acciones, inscripciones, asistencia y certificados segun el documento funcional `docs/funcional_gestor_voluntarios.md`.

## Estructura de la solucion

```
.
├── docs/funcional_gestor_voluntarios.md
├── src/
│   ├── Volun.Core/                # Dominio: entidades, enums, contratos, servicios
│   ├── Volun.Infrastructure/      # EF Core, contexto, identity, seed, repositorios
│   ├── Volun.Notifications/       # Servicio de correo (MailKit) + plantillas
│   └── Volun.Web/                 # API v1, Razor Pages de ejemplo, autenticacion, i18n
├── tests/
│   └── Volun.Tests/               # Pruebas unitarias e integradas (xUnit + WebApplicationFactory)
├── Directory.Build.props          # Configuracion comun (nullable, analyzers)
├── NuGet.config                   # Fuente nuget.org explicita
└── Volun.sln
```

## Puesta en marcha

1. **Restaurar y compilar**
   ```bash
   dotnet restore
   dotnet build
   ```
2. **Configurar la cadena de conexion**
   - Edita `src/Volun.Web/appsettings.Development.json` → `ConnectionStrings:DefaultConnection`.
3. **Ejecutar la migracion inicial**
   ```bash
   dotnet ef migrations add InitialCreate -p src/Volun.Infrastructure -s src/Volun.Web
   dotnet ef database update -p src/Volun.Infrastructure -s src/Volun.Web
   ```
4. **Levantar la API**
   ```bash
   dotnet run --project src/Volun.Web
   ```
   - Swagger: `https://localhost:5001/swagger`
   - Razor Pages de ejemplo: `/Acciones`, `/Panel`, `/Perfil`
5. **Ejecutar pruebas**
   ```bash
   dotnet test
   ```

> La inicializacion crea usuarios semilla:  
> - Admin → `admin@volun.local` / `ChangeThis!123`  
> - Coordinador → `coordinador@volun.local` / `ChangeThis!123`

## Endpoints y capacidades destacadas

- API REST `api/v1/...` con Minimal APIs, `ProblemDetails`, validaciones FluentValidation e identidad basada en roles.
- Acciones y turnos: CRUD, publicacion, control de cupos, restriccion por coordinador propietario.
- Voluntarios: alta publica, edicion, suspension (soft delete) y exportacion CSV filtrada por coordinador.
- Baja RGPD: `DELETE /api/v1/voluntarios/{id}` anonimiza el perfil y deja trazabilidad en auditoria (solo voluntario propietario o Admin).
- Inscripciones: creacion con lista de espera, cambio de estado, check-in/out con control de coordinador y endpoint de QR (`GET /api/v1/inscripciones/{id}/qr`).
- Certificados:
  - Administracion privada (`POST /api/v1/certificados`) con control de roles.
  - Descarga de PDF generado con QuestPDF (`GET /api/v1/certificados/{id}/pdf`).
  - Verificacion publica por codigo (`GET /api/v1/public/certificados/{codigo}`).
- Reportes: dashboard enriquecido (voluntarios activos, acciones por mes, top categorias, inscripciones por estado) respetando el alcance de cada coordinador.
- Exportaciones CSV (voluntarios/inscripciones/asistencias), filtradas autom�ticamente por coordinador y auditadas con los filtros usados.
- Integracion con QRCoder y QuestPDF para generar QR (PNG) y certificados PDF listos para descarga.

## Servicios y configuraciones clave

- Identity + roles (Admin, Coordinador, Voluntario, Invitado). Coordinadores solo operan sobre acciones e inscripciones propias.
- Localizacion ES/EN (`Localization/Shared.*.resx`).
- Servicio de correo (`EmailNotificationService`) configurable por `Notifications:Smtp`.
- QR y PDF reales registrados via DI (`IQrCodeGenerator`, `ICertificateService`).
- Licencia QuestPDF configurada para modo Community (`QuestPDF.Settings.License`).

## Siguientes pasos recomendados

- Permitir a los voluntarios descargar su propio QR desde un endpoint autenticado.
- Completar flujos de emision de certificados (notificaciones, versionado) y trazas de auditoria.
- Incorporar exportaciones XLSX y plantillas dinamicas de correo.
- Aumentar cobertura de pruebas (especialmente para certificados y reportes).
