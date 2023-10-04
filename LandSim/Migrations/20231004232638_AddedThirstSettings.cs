using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LandSim.Migrations
{
    /// <inheritdoc />
    public partial class AddedThirstSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "BaseThirstCost",
                table: "SimulationConfigs",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "DrinkThirstIncrease",
                table: "SimulationConfigs",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseThirstCost",
                table: "SimulationConfigs");

            migrationBuilder.DropColumn(
                name: "DrinkThirstIncrease",
                table: "SimulationConfigs");
        }
    }
}
