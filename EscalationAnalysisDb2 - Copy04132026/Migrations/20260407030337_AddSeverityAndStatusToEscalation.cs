using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EscalationAnalysisDb2.Migrations
{
    /// <inheritdoc />
    public partial class AddSeverityAndStatusToEscalation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseRecords_Severities_SeverityId",
                table: "CaseRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseRecords_Statuses_StatusId",
                table: "CaseRecords");

            migrationBuilder.AddColumn<int>(
                name: "SeverityId",
                table: "Escalations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "Escalations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Escalations_SeverityId",
                table: "Escalations",
                column: "SeverityId");

            migrationBuilder.CreateIndex(
                name: "IX_Escalations_StatusId",
                table: "Escalations",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseRecords_Severities_SeverityId",
                table: "CaseRecords",
                column: "SeverityId",
                principalTable: "Severities",
                principalColumn: "SeverityId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseRecords_Statuses_StatusId",
                table: "CaseRecords",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Escalations_Severities_SeverityId",
                table: "Escalations",
                column: "SeverityId",
                principalTable: "Severities",
                principalColumn: "SeverityId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Escalations_Statuses_StatusId",
                table: "Escalations",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseRecords_Severities_SeverityId",
                table: "CaseRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseRecords_Statuses_StatusId",
                table: "CaseRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Escalations_Severities_SeverityId",
                table: "Escalations");

            migrationBuilder.DropForeignKey(
                name: "FK_Escalations_Statuses_StatusId",
                table: "Escalations");

            migrationBuilder.DropIndex(
                name: "IX_Escalations_SeverityId",
                table: "Escalations");

            migrationBuilder.DropIndex(
                name: "IX_Escalations_StatusId",
                table: "Escalations");

            migrationBuilder.DropColumn(
                name: "SeverityId",
                table: "Escalations");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Escalations");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseRecords_Severities_SeverityId",
                table: "CaseRecords",
                column: "SeverityId",
                principalTable: "Severities",
                principalColumn: "SeverityId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseRecords_Statuses_StatusId",
                table: "CaseRecords",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
