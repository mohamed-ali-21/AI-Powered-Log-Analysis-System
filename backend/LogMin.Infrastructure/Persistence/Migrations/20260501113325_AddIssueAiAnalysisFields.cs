using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogMin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIssueAiAnalysisFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAiProcessed",
                table: "Issues",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Severity",
                table: "IssueAnalyses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "IssueAnalyses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "IssueAnalyses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Issues_IsAiProcessed",
                table: "Issues",
                column: "IsAiProcessed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Issues_IsAiProcessed",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "IsAiProcessed",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "IssueAnalyses");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "IssueAnalyses");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "IssueAnalyses");
        }
    }
}
