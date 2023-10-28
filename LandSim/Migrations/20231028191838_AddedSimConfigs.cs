using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LandSim.Migrations
{
    /// <inheritdoc />
    public partial class AddedSimConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaused",
                table: "SimulationConfigs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SimulationRate",
                table: "SimulationConfigs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaused",
                table: "SimulationConfigs");

            migrationBuilder.DropColumn(
                name: "SimulationRate",
                table: "SimulationConfigs");
        }
    }
}
