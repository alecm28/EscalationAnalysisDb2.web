using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EscalationAnalysisDb2.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEscalationTaskToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EscalationTask",
                table: "Escalations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "EscalationTask",
                table: "Escalations",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
