using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LandSim.Migrations
{
    /// <inheritdoc />
    public partial class MadeAgentIdUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AgentActions_AgentId",
                table: "AgentActions",
                column: "AgentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AgentActions_AgentId",
                table: "AgentActions");
        }
    }
}
