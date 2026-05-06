using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogMin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReshapeAgentSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentName",
                table: "AgentSettings");

            migrationBuilder.DropColumn(
                name: "BaseUrl",
                table: "AgentSettings");

            migrationBuilder.AddColumn<string>(
                name: "ApiServerUrl",
                table: "AgentSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiServerUrl",
                table: "AgentSettings");

            migrationBuilder.AddColumn<string>(
                name: "AgentName",
                table: "AgentSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BaseUrl",
                table: "AgentSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
