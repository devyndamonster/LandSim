using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LandSim.Migrations
{
    /// <inheritdoc />
    public partial class AddedPostbackUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostbackUrl",
                table: "AgentOwner",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostbackUrl",
                table: "AgentOwner");
        }
    }
}
