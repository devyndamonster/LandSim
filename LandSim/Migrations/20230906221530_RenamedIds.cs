using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LandSim.Migrations
{
    /// <inheritdoc />
    public partial class RenamedIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "GenerationSettings",
                newName: "GenerationSettingsId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ColorSelectors",
                newName: "ColorSelectorId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Colors",
                newName: "ColorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GenerationSettingsId",
                table: "GenerationSettings",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ColorSelectorId",
                table: "ColorSelectors",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ColorId",
                table: "Colors",
                newName: "Id");
        }
    }
}
