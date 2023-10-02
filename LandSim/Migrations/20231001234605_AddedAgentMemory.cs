using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LandSim.Migrations
{
    /// <inheritdoc />
    public partial class AddedAgentMemory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortTermMemory",
                table: "Agents",
                type: "TEXT",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedShortTermMemory",
                table: "AgentActions",
                type: "TEXT",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortTermMemory",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "UpdatedShortTermMemory",
                table: "AgentActions");
        }
    }
}
