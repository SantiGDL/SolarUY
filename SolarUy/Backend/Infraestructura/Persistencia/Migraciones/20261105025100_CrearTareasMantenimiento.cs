using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace SolarUy.Backend.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CrearTareasMantenimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TareasMantenimiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    TipoTarea = table.Column<int>(type: "int", nullable: false),
                    FechaCreada = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaFinalizado = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    FechaRealizada = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PanelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TareasMantenimiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TareasMantenimiento_Paneles_PanelId",
                        column: x => x.PanelId,
                        principalTable: "Paneles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TareasMantenimiento_Estado",
                table: "TareasMantenimiento",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_TareasMantenimiento_PanelId",
                table: "TareasMantenimiento",
                column: "PanelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TareasMantenimiento");
        }
    }
}
