using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarUy.Backend.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class HabilitacionPanelProduccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Deshabilitado",
                table: "Paneles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deshabilitado",
                table: "Paneles");
        }
    }
}
