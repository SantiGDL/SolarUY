using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace SolarUy.Backend.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarNodos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Nodos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ParqueId = table.Column<int>(type: "int", nullable: false),
                    UbicacionDentroParque = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nodos_Parques_ParqueId",
                        column: x => x.ParqueId,
                        principalTable: "Parques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Nodos_ParqueId",
                table: "Nodos",
                column: "ParqueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Nodos");
        }
    }
}
