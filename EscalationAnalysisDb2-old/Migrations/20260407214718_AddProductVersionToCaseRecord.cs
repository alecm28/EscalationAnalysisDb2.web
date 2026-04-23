using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EscalationAnalysisDb2.Migrations
{
    /// <inheritdoc />
    public partial class AddProductVersionToCaseRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductVersion",
                table: "CaseRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductVersion",
                table: "CaseRecords");
        }
    }
}
