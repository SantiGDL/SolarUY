using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace SolarUy.Backend.Modulos.Mediciones.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CrearMediciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Mediciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ParqueId = table.Column<int>(type: "int", nullable: false),
                    NodoId = table.Column<int>(type: "int", nullable: false),
                    PanelId = table.Column<int>(type: "int", nullable: false),
                    ValorGenerado = table.Column<double>(type: "double", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mediciones", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Mediciones_PanelId_FechaHora",
                table: "Mediciones",
                columns: new[] { "PanelId", "FechaHora" });

            migrationBuilder.CreateIndex(
                name: "IX_Mediciones_ParqueId_NodoId_FechaHora",
                table: "Mediciones",
                columns: new[] { "ParqueId", "NodoId", "FechaHora" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mediciones");
        }
    }
}
