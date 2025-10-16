# Documento funcional — Gestor de Voluntarios y Acciones de Voluntariado

**Proyecto:** Plataforma web para la gestión integral de voluntariado  
**Stack objetivo:** .NET 8 (ASP.NET Core MVC o Minimal APIs + Razor/Blazor Server), EF Core, SQL Server  
**Owner de negocio:** Departamento de Personas / Fundación  
**Estado:** Versión 1.0 (borrador funcional listo para pasar a desarrollo)  
**Glosario rápido:** Voluntario = persona inscrita; Acción = actividad/evento de voluntariado; ONG/Entidad = organizador; Coordinador = rol interno que gestiona acciones.

---

## 1) Objetivo y alcance

### 1.1 Objetivo
Desarrollar una plataforma que permita **captar**, **gestionar** y **movilizar** voluntarios en acciones puntuales o programas continuos, con trazabilidad de horas, certificaciones y reportes de impacto.

### 1.2 Alcance (V1)
- Alta y perfilado de **voluntarios** con verificación de email y consentimiento RGPD.
- Gestión de **acciones** (eventos, programas, campañas) con cupos, requisitos y turnos.
- **Inscripción** y **asignación** de voluntarios a acciones y turnos.
- **Check-in/Check-out** (presencial o por QR) para registrar horas y asistencia.
- **Comunicación** (email) transaccional básica: confirmaciones, avisos y recordatorios.
- **Cuadro de mando** para coordinadores (métricas básicas).
- **Exportaciones** (CSV/Excel) de voluntarios, acciones e inscripciones.
- **Auditoría** de cambios relevantes.
- **Roles y permisos**: Admin, Coordinador, Voluntario, Invitado.

### 1.3 Fuera de alcance (V1)
- Donaciones/pagos online.
- App móvil nativa (se prioriza web responsive).
- CRM avanzado o marketing automation.
- Firma electrónica avanzada (se deja trazabilidad simple).

---

## 2) Usuarios y roles

| Rol          | Descripción | Capacidades clave |
|--------------|-------------|-------------------|
| **Admin**    | Gobierno global del sistema | CRUD completo, configuración, seguridad, auditoría, informes globales |
| **Coordinador** | Gestiona acciones de su área | CRUD de acciones propias, gestión de inscripciones, check-in/out, exportaciones |
| **Voluntario** | Persona que participa | Registro/edición de perfil, inscripción, descargas de certificados, consulta de agenda |
| **Invitado** | Usuario no autenticado | Ver catálogo de acciones, registrarse como voluntario, solicitar inscripción |

**Matriz de permisos (resumen):**
- Admin: acceso total.
- Coordinador: solo acciones/inscripciones que coordina + lectura de catálogo global.
- Voluntario: sus datos + sus inscripciones y certificados.
- Invitado: lectura de acciones públicas + alta voluntario.

---

## 3) Requisitos no funcionales

- **Seguridad:** OAuth2/OIDC (Azure AD B2C o Identity), hashing de contraseñas, MFA opcional. Políticas de autorización por rol y por recurso.
- **RGPD:** consentimiento explícito, gestión de bajas/derecho al olvido, registro de consentimientos, DPA y retención configurable.
- **Escalabilidad:** arquitectura en capas, cacheo de catálogos, paginación en listados > 1000 filas.
- **Observabilidad:** logging estructurado (Serilog), métricas básicas y trazas de auditoría.
- **Calidad:** testing (unitario + integración en endpoints críticos), validaciones de dominio, DTOs y antiforgery en formularios.
- **Accesibilidad:** WCAG 2.1 AA.
- **Idiomas:** ES/EN (i18n con recursos).

---

## 4) Modelo de dominio (entidades V1)

### 4.1 Entidades y atributos (mínimos)
- **Voluntario**
  - `Id` (GUID), `Nombre`, `Apellidos`, `Email` (único), `Telefono`, `FechaNacimiento`, `DNI/NIE` (opcional), `Direccion` (opcional), `Provincia`, `Pais`, `Preferencias` (tags), `Habilidades` (tags), `Disponibilidad` (texto/slots), `ConsentimientoRGPD` (bool + fecha), `Estado` (Activo/Inactivo), `FechaAlta`, `FechaActualizacion`.
- **Accion** (acción de voluntariado)
  - `Id`, `Titulo`, `Descripcion`, `Organizador` (texto o relación futura con `Entidad`), `Ubicacion`, `Lat`, `Lng`, `Tipo` (Evento/Programa/Campaña), `Categoria` (tag), `Requisitos` (texto), `CupoMaximo`, `Visibilidad` (Publica/Privada), `FechaInicio`, `FechaFin`, `TurnosHabilitados` (bool), `Estado` (Borrador/Publicada/Cerrada), `CoordinadorId`.
- **Turno**
  - `Id`, `AccionId`, `Titulo`, `FechaInicio`, `FechaFin`, `Cupo`, `Notas`.
- **Inscripcion**
  - `Id`, `VoluntarioId`, `AccionId`, `TurnoId` (nullable si no hay turnos), `Estado` (Pendiente/Aprobada/ListaEspera/Cancelada/NoShow/Completada), `FechaSolicitud`, `FechaEstado`, `Notas`.
- **Asistencia**
  - `Id`, `InscripcionId`, `CheckIn` (fecha-hora), `CheckOut` (fecha-hora), `HorasComputadas` (decimal), `Metodo` (QR/Manual).
- **Certificado**
  - `Id`, `VoluntarioId`, `AccionId`, `Horas` (decimal), `EmitidoEn` (fecha), `CodigoVerificacion` (string único).
- **UsuarioSistema** (si se separa de Voluntario)
  - `Id`, `Email`, `Rol`, `VoluntarioId` (opcional si un voluntario también accede).
- **Auditoria**
  - `Id`, `Entidad`, `EntidadId`, `Accion` (Create/Update/Delete), `Usuario`, `Fecha`, `ValorAnterior` (json), `ValorNuevo` (json), `Ip`.

### 4.2 Relaciones clave
- `Voluntario 1..* Inscripcion`  
- `Accion 1..* Turno` y `Accion 1..* Inscripcion`  
- `Turno 1..* Inscripcion` (opcional)  
- `Inscripcion 1..1 Asistencia` (por cada participación)  
- `Voluntario 1..* Certificado`  
- `Coordinador (UsuarioSistema) 1..* Accion`

---

## 5) Casos de uso (V1)

1. Alta de voluntario con verificación de email y consentimiento RGPD.
2. Edición de perfil, habilidades y disponibilidad.
3. Catálogo público de acciones con filtros (fecha, categoría, ubicación).
4. Detalle de acción con requisitos, turnos y cupos.
5. Solicitud de inscripción (acción o turno).
6. Gestión de inscripciones por coordinador (aprobar, lista de espera, cancelar).
7. Emisión de QR personal de inscripción para check-in (único por voluntario/turno).
8. Check-in y check-out en la acción (lector QR o panel coordinador).
9. Cómputo de horas y generación de certificado en PDF con código de verificación.
10. Panel de métricas (voluntarios activos, horas totales, asistencia por acción).
11. Exportación CSV/Excel de voluntarios/inscripciones/asistencias.
12. Motor de notificaciones (confirmación, recordatorios, cambios de estado).
13. Búsqueda avanzada por nombre, email, DNI, tags y estado.
14. Auditoría de cambios en acciones e inscripciones.
15. Solicitud de baja/olvido de datos (cumplimiento RGPD).

---

## 6) Reglas de negocio (extracto)

- **Cupos:** no se puede aprobar una inscripción si el cupo del turno/acción está completo (se mueve a lista de espera).
- **Estados de inscripción:** flujo permitido: Pendiente → (Aprobada | ListaEspera | Cancelada). Aprobada → (Completada | NoShow | Cancelada).
- **Horas:** `HorasComputadas = max(0, CheckOut - CheckIn)` en horas decimales, redondeo a 2 decimales.
- **Certificados:** solo se emiten si la inscripción está **Completada** y las horas > 0.
- **Visibilidad:** acciones privadas solo visibles con enlace o para roles con permiso.
- **Turnos:** si hay turnos habilitados, la inscripción debe apuntar a un turno específico.
- **RGPD:** el voluntario puede descargar sus datos y solicitar borrado (anonimización de históricos).

---

## 7) API (borrador)

**Convención:** `/api/v1` — JSON — JWT Bearer.  
**Códigos:** 200/201/204/400/401/403/404/409/422/500.

- `POST /auth/login` (si no hay SSO)
- `GET /voluntarios?query=&estado=&page=&size=`
- `POST /voluntarios`
- `GET /voluntarios/{id}`
- `PUT /voluntarios/{id}`
- `DELETE /voluntarios/{id}` (soft delete o anonimizar)
- `GET /acciones?desde=&hasta=&categoria=&ubicacion=&page=&size=`
- `POST /acciones`
- `GET /acciones/{id}`
- `PUT /acciones/{id}`
- `POST /acciones/{id}/turnos`
- `GET /acciones/{id}/turnos`
- `POST /inscripciones` (body: VoluntarioId, AccionId, TurnoId?)
- `PUT /inscripciones/{id}/estado` (Aprobar/ListaEspera/Cancelar/Completada/NoShow)
- `POST /inscripciones/{id}/qr` (genera/renueva QR)
- `POST /asistencias/checkin` (body: InscripcionId o QR)
- `POST /asistencias/checkout` (body: InscripcionId o QR)
- `GET /certificados/{codigoVerificacion}` (público verificación)
- `GET /reportes/dashboard?desde=&hasta=`
- `GET /export/voluntarios|inscripciones|asistencias?formato=csv|xlsx`

**Errores comunes:**
- `409 Conflict` al exceder cupos.
- `422 UnprocessableEntity` validaciones de dominio.
- `403 Forbidden` por permisos/ownership de acción.

---

## 8) Interfaz (MVP web)

- **Catálogo** de acciones (cards + filtros).
- **Detalle** de acción con turnos, requisitos y CTA “Inscribirme”.
- **Mi perfil** voluntario: datos, preferencias, disponibilidad, mis inscripciones, mis certificados.
- **Panel coordinador:** listado de acciones propias, inscripciones, check-in/out, exportaciones.
- **Admin:** gestión global, seguridad, paramétricas (categorías, tags), auditoría.

**QR y asistencia:**
- Vista “Puesto de control” para coordinadores: input de lector QR (teclado) + tabla de últimos check-ins.
- Alternativa manual por buscador (email/DNI).

---

## 9) Notificaciones (plantillas mínimas)

- **Confirmación de registro** de voluntario.
- **Confirmación de solicitud** de inscripción.
- **Aprobación / Lista de espera / Cancelación** de inscripción.
- **Recordatorio** 48h antes del inicio.
- **Resumen** tras la acción con horas y link a certificado (si aplica).

Parámetros SMTP/API (SendGrid/SMTP corporativo) configurables en `appsettings.*.json`.

---

## 10) Datos y migraciones

- Base de datos: SQL Server (tables: `Voluntarios`, `Acciones`, `Turnos`, `Inscripciones`, `Asistencias`, `Certificados`, `Usuarios`, `Auditoria`).  
- Índices sugeridos: `Voluntarios.Email (unique)`, `Inscripciones(VoluntarioId,AccionId,Estado)`, `Turnos(AccionId,FechaInicio)`.  
- Soft-delete o columnas `Activo/Estado` donde aplique.  
- Semillas (seed) mínimas: Admin, categorías por defecto.

---

## 11) Seguridad y cumplimiento

- Política de contraseñas, lockout y MFA opcional.
- Consentimiento RGPD almacenado con timestamp e IP.
- **Derecho al olvido**: anonimización de atributos personales manteniendo métricas agregadas.
- Registro de acceso y cambios (auditoría).

---

## 12) Métricas (MVP)

- Voluntarios activos (últimos 12 meses).
- Acciones publicadas por mes.
- Tasa de aprobación/NoShow.
- Horas totales por periodo y por acción.
- Top categorías/ubicaciones.

---

## 13) Criterios de aceptación (ejemplos)

- **Inscripción con cupo lleno** → sistema devuelve `409` y coloca al voluntario en **Lista de espera**.
- **Check-in/Check-out por QR** → registra hora exacta y computa horas. Si falta checkout, permite cierre manual por coordinador.
- **Certificado** → descargable en PDF y verificable con URL pública usando `CodigoVerificacion` único.
- **Exportación** → genera CSV válido en <5s hasta 50k registros con paginación de backend.

---

## 14) Roadmap lógico de desarrollo (sin semanas)

1. Bootstrap del proyecto (.NET + EF Core + Identity/SSO, configuración básica).
2. Modelo de datos y migraciones iniciales.
3. Autenticación/Autorización y roles.
4. CRUD de **Voluntarios** (API + vistas).
5. CRUD de **Acciones** y **Turnos** (API + vistas).
6. **Inscripciones** (flujo completo + validaciones de cupo/estados).
7. **Asistencias** con QR y panel de control.
8. **Certificados** (generación PDF y verificación pública).
9. **Notificaciones** (SMTP/API) y plantillas.
10. **Dashboard** básico y exportaciones CSV/XLSX.
11. **Auditoría** y RGPD (descarga/borrado/anonimización).
12. Accesibilidad, i18n y mejora UX.
13. Tests (unitarios/integración) y hardening seguridad.
14. Despliegue (CI/CD) y documentación operativa.

---

## 15) Requisitos técnicos (guía para desarrollo)

- **Proyectos sugeridos (solution):**
  - `Volun.Core` (dominio + contratos)
  - `Volun.Infrastructure` (EF Core, repos, migraciones)
  - `Volun.Web` (API + MVC/Blazor)
  - `Volun.Notifications` (servicio plantillas/SMTP)
  - `Volun.Tests` (xUnit + WebApplicationFactory)

- **Paquetes:** `Microsoft.EntityFrameworkCore.SqlServer`, `Serilog.AspNetCore`, `FluentValidation`, `ClosedXML` (export xlsx), `QuestPDF` o `DinkToPdf` (PDF), `QRCoder` (QR).

- **Convenciones:** DTOs para API, AutoMapper opcional, paginación `page/size`, filtros por query params, `ProblemDetails` para errores.

---

## 16) Mockups/UX (wireframes de referencia rápida)

- **/acciones** (grid de cards) → filtros a la izquierda (fecha, categoría, ubicación).  
- **/acciones/{id}** → detalle + lista de turnos + botón “Inscribirme”.  
- **/panel/coordinador** → pestañas: Acciones | Inscripciones | Check-in | Exportar.  
- **/perfil** → datos personales, preferencias, disponibilidad, certificados.

*(Los mockups visuales se entregarán en un anexo si es necesario.)*

---

## 17) Riesgos y supuestos

- **Riesgo:** saturación de cupos en pico → cola de espera y comunicación clara.
- **Riesgo:** lectores QR heterogéneos → usar input compatible “teclado”.  
- **Supuesto:** habrá SMTP/API corporativa y subdominio dedicado.  
- **Supuesto:** coordinadores conocen RGPD básico y normas del programa.

---

## 18) Preguntas abiertas (para cerrar con negocio)

1. ¿Se requiere doble opt-in de email en el alta de voluntario?
2. ¿Certificados con firma digital/e-seal o solo código verificable?
3. ¿Turnos solapados permitidos para el mismo voluntario?
4. ¿Tiempos de retención de datos por defecto (p.ej., 36 meses)?
5. ¿Integración con calendarios (ICS) para voluntarios y coordinadores?

---

**Fin del documento funcional (V1.0).**  
Listo para importarlo en VS Code y generar el scaffolding.
