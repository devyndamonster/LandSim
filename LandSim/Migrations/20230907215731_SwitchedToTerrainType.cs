using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LandSim.Migrations
{
    /// <inheritdoc />
    public partial class SwitchedToTerrainType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ColorSelectors");

            migrationBuilder.DropTable(
                name: "Colors");

            migrationBuilder.CreateTable(
                name: "TerrainSelectors",
                columns: table => new
                {
                    TerrainSelectorId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaxValue = table.Column<float>(type: "REAL", nullable: false),
                    MinValue = table.Column<float>(type: "REAL", nullable: false),
                    TerrainType = table.Column<int>(type: "INTEGER", nullable: false),
                    GenerationSettingsId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerrainSelectors", x => x.TerrainSelectorId);
                    table.ForeignKey(
                        name: "FK_TerrainSelectors_GenerationSettings_GenerationSettingsId",
                        column: x => x.GenerationSettingsId,
                        principalTable: "GenerationSettings",
                        principalColumn: "GenerationSettingsId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TerrainSelectors_GenerationSettingsId",
                table: "TerrainSelectors",
                column: "GenerationSettingsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TerrainSelectors");

            migrationBuilder.CreateTable(
                name: "Colors",
                columns: table => new
                {
                    ColorId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Blue = table.Column<float>(type: "REAL", nullable: false),
                    Green = table.Column<float>(type: "REAL", nullable: false),
                    Red = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colors", x => x.ColorId);
                });

            migrationBuilder.CreateTable(
                name: "ColorSelectors",
                columns: table => new
                {
                    ColorSelectorId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ColorId = table.Column<int>(type: "INTEGER", nullable: false),
                    GenerationSettingsId = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxValue = table.Column<float>(type: "REAL", nullable: false),
                    MinValue = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorSelectors", x => x.ColorSelectorId);
                    table.ForeignKey(
                        name: "FK_ColorSelectors_Colors_ColorId",
                        column: x => x.ColorId,
                        principalTable: "Colors",
                        principalColumn: "ColorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ColorSelectors_GenerationSettings_GenerationSettingsId",
                        column: x => x.GenerationSettingsId,
                        principalTable: "GenerationSettings",
                        principalColumn: "GenerationSettingsId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ColorSelectors_ColorId",
                table: "ColorSelectors",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_ColorSelectors_GenerationSettingsId",
                table: "ColorSelectors",
                column: "GenerationSettingsId");
        }
    }
}
