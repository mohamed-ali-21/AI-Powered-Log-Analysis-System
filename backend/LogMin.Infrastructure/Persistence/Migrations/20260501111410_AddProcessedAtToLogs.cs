using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogMin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedAtToLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "Logs",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "Logs");
        }
    }
}
