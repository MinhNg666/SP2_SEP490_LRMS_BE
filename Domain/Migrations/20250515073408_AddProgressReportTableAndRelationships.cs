using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddProgressReportTableAndRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "progress_report_id",
                table: "VoteResult",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "progress_report_id",
                table: "Fund_Disbursement",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "progress_report_id",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "progress_report_id",
                table: "CouncilVote",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "progress_report_id",
                table: "AssignReview",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProgressReport",
                columns: table => new
                {
                    ProgressReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ProjectPhaseId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressReport", x => x.ProgressReportId);
                    table.ForeignKey(
                        name: "FK_ProgressReport_ProjectPhase_ProjectPhaseId",
                        column: x => x.ProjectPhaseId,
                        principalTable: "ProjectPhase",
                        principalColumn: "project_phase_id");
                    table.ForeignKey(
                        name: "FK_ProgressReport_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VoteResult_progress_report_id",
                table: "VoteResult",
                column: "progress_report_id");

            migrationBuilder.CreateIndex(
                name: "IX_Fund_Disbursement_progress_report_id",
                table: "Fund_Disbursement",
                column: "progress_report_id");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_progress_report_id",
                table: "Documents",
                column: "progress_report_id");

            migrationBuilder.CreateIndex(
                name: "IX_CouncilVote_progress_report_id",
                table: "CouncilVote",
                column: "progress_report_id");

            migrationBuilder.CreateIndex(
                name: "IX_AssignReview_progress_report_id",
                table: "AssignReview",
                column: "progress_report_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressReport_ProjectId",
                table: "ProgressReport",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressReport_ProjectPhaseId",
                table: "ProgressReport",
                column: "ProjectPhaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignReview_ProgressReport_progress_report_id",
                table: "AssignReview",
                column: "progress_report_id",
                principalTable: "ProgressReport",
                principalColumn: "ProgressReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_CouncilVote_ProgressReport_progress_report_id",
                table: "CouncilVote",
                column: "progress_report_id",
                principalTable: "ProgressReport",
                principalColumn: "ProgressReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_ProgressReport_progress_report_id",
                table: "Documents",
                column: "progress_report_id",
                principalTable: "ProgressReport",
                principalColumn: "ProgressReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_Fund_Disbursement_ProgressReport_progress_report_id",
                table: "Fund_Disbursement",
                column: "progress_report_id",
                principalTable: "ProgressReport",
                principalColumn: "ProgressReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_VoteResult_ProgressReport_progress_report_id",
                table: "VoteResult",
                column: "progress_report_id",
                principalTable: "ProgressReport",
                principalColumn: "ProgressReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignReview_ProgressReport_progress_report_id",
                table: "AssignReview");

            migrationBuilder.DropForeignKey(
                name: "FK_CouncilVote_ProgressReport_progress_report_id",
                table: "CouncilVote");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_ProgressReport_progress_report_id",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Fund_Disbursement_ProgressReport_progress_report_id",
                table: "Fund_Disbursement");

            migrationBuilder.DropForeignKey(
                name: "FK_VoteResult_ProgressReport_progress_report_id",
                table: "VoteResult");

            migrationBuilder.DropTable(
                name: "ProgressReport");

            migrationBuilder.DropIndex(
                name: "IX_VoteResult_progress_report_id",
                table: "VoteResult");

            migrationBuilder.DropIndex(
                name: "IX_Fund_Disbursement_progress_report_id",
                table: "Fund_Disbursement");

            migrationBuilder.DropIndex(
                name: "IX_Documents_progress_report_id",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_CouncilVote_progress_report_id",
                table: "CouncilVote");

            migrationBuilder.DropIndex(
                name: "IX_AssignReview_progress_report_id",
                table: "AssignReview");

            migrationBuilder.DropColumn(
                name: "progress_report_id",
                table: "VoteResult");

            migrationBuilder.DropColumn(
                name: "progress_report_id",
                table: "Fund_Disbursement");

            migrationBuilder.DropColumn(
                name: "progress_report_id",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "progress_report_id",
                table: "CouncilVote");

            migrationBuilder.DropColumn(
                name: "progress_report_id",
                table: "AssignReview");
        }
    }
}
