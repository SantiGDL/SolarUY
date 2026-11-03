using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace SolarUy.Backend.Infraestructura.Persistencia.Migraciones
{
    public partial class AgregarPaneles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Paneles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    PotenciaActual = table.Column<double>(type: "double", nullable: false),
                    Orientacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    NodoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paneles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Paneles_Nodos_NodoId",
                        column: x => x.NodoId,
                        principalTable: "Nodos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Paneles_NodoId",
                table: "Paneles",
                column: "NodoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Paneles");
        }
    }
}
