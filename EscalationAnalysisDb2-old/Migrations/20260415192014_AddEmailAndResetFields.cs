using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EscalationAnalysisDb2.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailAndResetFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResetToken",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpiry",
                table: "AppUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "ResetToken",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "ResetTokenExpiry",
                table: "AppUsers");
        }
    }
}
