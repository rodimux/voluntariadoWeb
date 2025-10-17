using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Volun.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "volun");

            migrationBuilder.CreateTable(
                name: "Acciones",
                schema: "volun",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Organizador = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Latitud = table.Column<double>(type: "float", nullable: true),
                    Longitud = table.Column<double>(type: "float", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Requisitos = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    CupoMaximo = table.Column<int>(type: "int", nullable: false),
                    Visibilidad = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FechaInicio = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FechaFin = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TurnosHabilitados = table.Column<bool>(type: "bit", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CoordinadorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "volun",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "volun",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoluntarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Auditoria",
                schema: "volun",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Entidad = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EntidadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Datos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Voluntarios",
                schema: "volun",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    FechaNacimiento = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DniNie = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Provincia = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Pais = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Disponibilidad = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ConsentimientoRgpd = table.Column<bool>(type: "bit", nullable: false),
                    ConsentimientoRgpdFecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaAlta = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FechaActualizacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Habilidades = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Preferencias = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voluntarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Turnos",
                schema: "volun",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FechaInicio = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FechaFin = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Cupo = table.Column<int>(type: "int", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turnos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Turnos_Acciones_AccionId",
                        column: x => x.AccionId,
                        principalSchema: "volun",
                        principalTable: "Acciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "volun",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "volun",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "volun",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "volun",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "volun",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "volun",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "volun",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "volun",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "volun",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "volun",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "volun",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Certificados",
                schema: "volun",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoluntarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Horas = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    EmitidoEn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CodigoVerificacion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UrlPublica = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificados_Acciones_AccionId",
                        column: x => x.AccionId,
                        principalSchema: "volun",
                        principalTable: "Acciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Certificados_Voluntarios_VoluntarioId",
                        column: x => x.VoluntarioId,
                        principalSchema: "volun",
                        principalTable: "Voluntarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosSistema",
                schema: "volun",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    VoluntarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosSistema", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosSistema_Voluntarios_VoluntarioId",
                        column: x => x.VoluntarioId,
                        principalSchema: "volun",
                        principalTable: "Voluntarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inscripciones",
                schema: "volun",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoluntarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TurnoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FechaSolicitud = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FechaEstado = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ComentariosEstado = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    QrToken = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inscripciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inscripciones_Acciones_AccionId",
                        column: x => x.AccionId,
                        principalSchema: "volun",
                        principalTable: "Acciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inscripciones_Turnos_TurnoId",
                        column: x => x.TurnoId,
                        principalSchema: "volun",
                        principalTable: "Turnos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Inscripciones_Voluntarios_VoluntarioId",
                        column: x => x.VoluntarioId,
                        principalSchema: "volun",
                        principalTable: "Voluntarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Asistencias",
                schema: "volun",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InscripcionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CheckIn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CheckOut = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    HorasComputadas = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Metodo = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Comentarios = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asistencias_Inscripciones_InscripcionId",
                        column: x => x.InscripcionId,
                        principalSchema: "volun",
                        principalTable: "Inscripciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_InscripcionId",
                schema: "volun",
                table: "Asistencias",
                column: "InscripcionId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "volun",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "volun",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "volun",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "volun",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "volun",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "volun",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "volun",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Certificados_AccionId",
                schema: "volun",
                table: "Certificados",
                column: "AccionId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificados_CodigoVerificacion",
                schema: "volun",
                table: "Certificados",
                column: "CodigoVerificacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificados_VoluntarioId",
                schema: "volun",
                table: "Certificados",
                column: "VoluntarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_AccionId",
                schema: "volun",
                table: "Inscripciones",
                column: "AccionId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_TurnoId",
                schema: "volun",
                table: "Inscripciones",
                column: "TurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_VoluntarioId_AccionId",
                schema: "volun",
                table: "Inscripciones",
                columns: new[] { "VoluntarioId", "AccionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_AccionId",
                schema: "volun",
                table: "Turnos",
                column: "AccionId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSistema_Email",
                schema: "volun",
                table: "UsuariosSistema",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSistema_VoluntarioId",
                schema: "volun",
                table: "UsuariosSistema",
                column: "VoluntarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Voluntarios_Email",
                schema: "volun",
                table: "Voluntarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asistencias",
                schema: "volun");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "volun");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "volun");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "volun");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "volun");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "volun");

            migrationBuilder.DropTable(
                name: "Auditoria",
                schema: "volun");

            migrationBuilder.DropTable(
                name: "Certificados",
                schema: "volun");

            migrationBuilder.DropTable(
                name: "UsuariosSistema",
                schema: "volun");

            migrationBuilder.DropTable(
                name: "Inscripciones",
                schema: "volun");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "volun");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "volun");

            migrationBuilder.DropTable(
                name: "Turnos",
                schema: "volun");

            migrationBuilder.DropTable(
                name: "Voluntarios",
                schema: "volun");

            migrationBuilder.DropTable(
                name: "Acciones",
                schema: "volun");
        }
    }
}
