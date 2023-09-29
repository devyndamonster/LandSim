using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LandSim.Migrations
{
    /// <inheritdoc />
    public partial class AddedSimulationConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SimulationConfigs",
                columns: table => new
                {
                    SimulationConfigId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConsumableSpawnChance = table.Column<float>(type: "REAL", nullable: false),
                    ConsumableHungerIncrease = table.Column<float>(type: "REAL", nullable: false),
                    ConsumableVegitationSpawnThreshold = table.Column<float>(type: "REAL", nullable: false),
                    BaseHungerCost = table.Column<float>(type: "REAL", nullable: false),
                    MovementHungerCost = table.Column<float>(type: "REAL", nullable: false),
                    ClimbHungerCost = table.Column<float>(type: "REAL", nullable: false),
                    AgentSpawnChange = table.Column<float>(type: "REAL", nullable: false),
                    VegitationMovementHungerCost = table.Column<float>(type: "REAL", nullable: false),
                    VegitationSpawnChance = table.Column<float>(type: "REAL", nullable: false),
                    VegitationSpreadChance = table.Column<float>(type: "REAL", nullable: false),
                    VegitationGrowthRate = table.Column<float>(type: "REAL", nullable: false),
                    VegitationDecreaseFromMovement = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulationConfigs", x => x.SimulationConfigId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SimulationConfigs");
        }
    }
}
