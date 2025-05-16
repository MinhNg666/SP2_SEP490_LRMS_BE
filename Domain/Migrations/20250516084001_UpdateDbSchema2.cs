using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDbSchema2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignReview_ProgressReport_progress_report_id",
                table: "AssignReview");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_CompletionRequestDetails_ProjectRequests_request_id",
            //    table: "CompletionRequestDetails");

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
                name: "FK_ProgressReport_ProjectPhase_ProjectPhaseId",
                table: "ProgressReport");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgressReport_Projects_ProjectId",
                table: "ProgressReport");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequests_Fund_Disbursement_fund_disbursement_id",
                table: "ProjectRequests");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_ProjectRequests_Groups_assigned_council",
            //    table: "ProjectRequests");

            // migrationBuilder.DropForeignKey(
            //     name: "FK_ProjectRequests_ProjectPhase_phase_id",
            //     table: "ProjectRequests");

            // migrationBuilder.DropForeignKey(
            //     name: "FK_ProjectRequests_Projects_project_id",
            //     table: "ProjectRequests");

            // migrationBuilder.DropForeignKey(
            //     name: "FK_ProjectRequests_Timeline_timeline_id",
            //     table: "ProjectRequests");

            // migrationBuilder.DropForeignKey(
            //     name: "FK_ProjectRequests_Users_approved_by",
            //     table: "ProjectRequests");

            // migrationBuilder.DropForeignKey(
            //     name: "FK_ProjectRequests_Users_requested_by",
            //     table: "ProjectRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_VoteResult_ProgressReport_progress_report_id",
                table: "VoteResult");

            migrationBuilder.AddColumn<int>(
                name: "assign_review_id",
                table: "VoteResult",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "decided_at",
                table: "VoteResult",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "final_comment",
                table: "VoteResult",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "number_conference",
                table: "Quotas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "number_paper",
                table: "Quotas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "inspection_id",
                table: "ProjectRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "progress_report_id",
                table: "ProjectRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "journal_abstract",
                table: "Journal",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "project_phase_id",
                table: "Journal",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "inspection_status",
                table: "Inspection",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "proposed_research_resource_id",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "research_resource_id",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "conference_DOI",
                table: "Conference",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "conference_abstract",
                table: "Conference",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "conference_link",
                table: "Conference",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "project_phase_id",
                table: "Conference",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "group_id",
                table: "AssignReview",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProposedResearchResource",
                columns: table => new
                {
                    proposed_resource_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proposed_resource_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    proposed_resource_quantity = table.Column<int>(type: "int", nullable: true),
                    proposed_resource_cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    proposed_resource_type = table.Column<int>(type: "int", nullable: true),
                    proposed_resource_status = table.Column<int>(type: "int", nullable: true),
                    project_request_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposedResearchResource", x => x.proposed_resource_id);
                    table.ForeignKey(
                        name: "FK_ProposedResearchResource_ProjectRequest",
                        column: x => x.project_request_id,
                        principalTable: "ProjectRequests",
                        principalColumn: "request_id");
                });

            migrationBuilder.CreateTable(
                name: "ResearchResource",
                columns: table => new
                {
                    resource_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    resource_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    resource_quantity = table.Column<int>(type: "int", nullable: true),
                    resource_cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    resource_type = table.Column<int>(type: "int", nullable: true),
                    resource_status = table.Column<int>(type: "int", nullable: true),
                    project_phase_id = table.Column<int>(type: "int", nullable: true),
                    project_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchResource", x => x.resource_id);
                    table.ForeignKey(
                        name: "FK_ResearchResource_Project",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "project_id");
                    table.ForeignKey(
                        name: "FK_ResearchResource_ProjectPhase",
                        column: x => x.project_phase_id,
                        principalTable: "ProjectPhase",
                        principalColumn: "project_phase_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_VoteResult_assign_review_id",
                table: "VoteResult",
                column: "assign_review_id",
                unique: true,
                filter: "[assign_review_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequests_inspection_id",
                table: "ProjectRequests",
                column: "inspection_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequests_progress_report_id",
                table: "ProjectRequests",
                column: "progress_report_id");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_project_phase_id",
                table: "Journal",
                column: "project_phase_id");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_proposed_research_resource_id",
                table: "Documents",
                column: "proposed_research_resource_id");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_research_resource_id",
                table: "Documents",
                column: "research_resource_id");

            migrationBuilder.CreateIndex(
                name: "IX_Conference_project_phase_id",
                table: "Conference",
                column: "project_phase_id");

            migrationBuilder.CreateIndex(
                name: "IX_AssignReview_group_id",
                table: "AssignReview",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProposedResearchResource_project_request_id",
                table: "ProposedResearchResource",
                column: "project_request_id");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchResource_project_id",
                table: "ResearchResource",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchResource_project_phase_id",
                table: "ResearchResource",
                column: "project_phase_id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignReview_Group",
                table: "AssignReview",
                column: "group_id",
                principalTable: "Groups",
                principalColumn: "group_id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignReview_ProgressReport",
                table: "AssignReview",
                column: "progress_report_id",
                principalTable: "ProgressReport",
                principalColumn: "ProgressReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompletionRequestDetails_ProjectRequests_request_id",
                table: "CompletionRequestDetails",
                column: "request_id",
                principalTable: "ProjectRequests",
                principalColumn: "request_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Conference_ProjectPhase",
                table: "Conference",
                column: "project_phase_id",
                principalTable: "ProjectPhase",
                principalColumn: "project_phase_id");

            migrationBuilder.AddForeignKey(
                name: "FK_CouncilVote_ProgressReport",
                table: "CouncilVote",
                column: "progress_report_id",
                principalTable: "ProgressReport",
                principalColumn: "ProgressReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_ProgressReport",
                table: "Documents",
                column: "progress_report_id",
                principalTable: "ProgressReport",
                principalColumn: "ProgressReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_ProposedResearchResource",
                table: "Documents",
                column: "proposed_research_resource_id",
                principalTable: "ProposedResearchResource",
                principalColumn: "proposed_resource_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_ResearchResource",
                table: "Documents",
                column: "research_resource_id",
                principalTable: "ResearchResource",
                principalColumn: "resource_id");

            migrationBuilder.AddForeignKey(
                name: "FK_FundDisbursement_ProgressReport",
                table: "Fund_Disbursement",
                column: "progress_report_id",
                principalTable: "ProgressReport",
                principalColumn: "ProgressReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_Journal_ProjectPhase",
                table: "Journal",
                column: "project_phase_id",
                principalTable: "ProjectPhase",
                principalColumn: "project_phase_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressReport_Project",
                table: "ProgressReport",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "project_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressReport_ProjectPhase",
                table: "ProgressReport",
                column: "ProjectPhaseId",
                principalTable: "ProjectPhase",
                principalColumn: "project_phase_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequest_FundDisbursement",
                table: "ProjectRequests",
                column: "fund_disbursement_id",
                principalTable: "Fund_Disbursement",
                principalColumn: "fund_disbursement_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequest_Group_AssignedCouncil",
                table: "ProjectRequests",
                column: "assigned_council",
                principalTable: "Groups",
                principalColumn: "group_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequest_Inspection",
                table: "ProjectRequests",
                column: "inspection_id",
                principalTable: "Inspection",
                principalColumn: "inspection_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequest_ProgressReport",
                table: "ProjectRequests",
                column: "progress_report_id",
                principalTable: "ProgressReport",
                principalColumn: "ProgressReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequest_Project",
                table: "ProjectRequests",
                column: "project_id",
                principalTable: "Projects",
                principalColumn: "project_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequest_ProjectPhase",
                table: "ProjectRequests",
                column: "phase_id",
                principalTable: "ProjectPhase",
                principalColumn: "project_phase_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequest_Timeline",
                table: "ProjectRequests",
                column: "timeline_id",
                principalSchema: "dbo",
                principalTable: "Timeline",
                principalColumn: "timeline_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequest_User_ApprovedBy",
                table: "ProjectRequests",
                column: "approved_by",
                principalTable: "Users",
                principalColumn: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequest_User_RequestedBy",
                table: "ProjectRequests",
                column: "requested_by",
                principalTable: "Users",
                principalColumn: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_VoteResult_AssignReview_assign_review_id",
                table: "VoteResult",
                column: "assign_review_id",
                principalTable: "AssignReview",
                principalColumn: "assign_id");

            migrationBuilder.AddForeignKey(
                name: "FK_VoteResult_ProgressReport",
                table: "VoteResult",
                column: "progress_report_id",
                principalTable: "ProgressReport",
                principalColumn: "ProgressReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignReview_Group",
                table: "AssignReview");

            migrationBuilder.DropForeignKey(
                name: "FK_AssignReview_ProgressReport",
                table: "AssignReview");

            migrationBuilder.DropForeignKey(
                name: "FK_CompletionRequestDetails_ProjectRequests_request_id",
                table: "CompletionRequestDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Conference_ProjectPhase",
                table: "Conference");

            migrationBuilder.DropForeignKey(
                name: "FK_CouncilVote_ProgressReport",
                table: "CouncilVote");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_ProgressReport",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_ProposedResearchResource",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_ResearchResource",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_FundDisbursement_ProgressReport",
                table: "Fund_Disbursement");

            migrationBuilder.DropForeignKey(
                name: "FK_Journal_ProjectPhase",
                table: "Journal");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgressReport_Project",
                table: "ProgressReport");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgressReport_ProjectPhase",
                table: "ProgressReport");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequest_FundDisbursement",
                table: "ProjectRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequest_Group_AssignedCouncil",
                table: "ProjectRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequest_Inspection",
                table: "ProjectRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequest_ProgressReport",
                table: "ProjectRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequest_Project",
                table: "ProjectRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequest_ProjectPhase",
                table: "ProjectRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequest_Timeline",
                table: "ProjectRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequest_User_ApprovedBy",
                table: "ProjectRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequest_User_RequestedBy",
                table: "ProjectRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_VoteResult_AssignReview_assign_review_id",
                table: "VoteResult");

            migrationBuilder.DropForeignKey(
                name: "FK_VoteResult_ProgressReport",
                table: "VoteResult");

            migrationBuilder.DropTable(
                name: "ProposedResearchResource");

            migrationBuilder.DropTable(
                name: "ResearchResource");

            migrationBuilder.DropIndex(
                name: "IX_VoteResult_assign_review_id",
                table: "VoteResult");

            migrationBuilder.DropIndex(
                name: "IX_ProjectRequests_inspection_id",
                table: "ProjectRequests");

            migrationBuilder.DropIndex(
                name: "IX_ProjectRequests_progress_report_id",
                table: "ProjectRequests");

            migrationBuilder.DropIndex(
                name: "IX_Journal_project_phase_id",
                table: "Journal");

            migrationBuilder.DropIndex(
                name: "IX_Documents_proposed_research_resource_id",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_research_resource_id",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Conference_project_phase_id",
                table: "Conference");

            migrationBuilder.DropIndex(
                name: "IX_AssignReview_group_id",
                table: "AssignReview");

            migrationBuilder.DropColumn(
                name: "assign_review_id",
                table: "VoteResult");

            migrationBuilder.DropColumn(
                name: "decided_at",
                table: "VoteResult");

            migrationBuilder.DropColumn(
                name: "final_comment",
                table: "VoteResult");

            migrationBuilder.DropColumn(
                name: "number_conference",
                table: "Quotas");

            migrationBuilder.DropColumn(
                name: "number_paper",
                table: "Quotas");

            migrationBuilder.DropColumn(
                name: "inspection_id",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "progress_report_id",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "journal_abstract",
                table: "Journal");

            migrationBuilder.DropColumn(
                name: "project_phase_id",
                table: "Journal");

            migrationBuilder.DropColumn(
                name: "inspection_status",
                table: "Inspection");

            migrationBuilder.DropColumn(
                name: "proposed_research_resource_id",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "research_resource_id",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "conference_DOI",
                table: "Conference");

            migrationBuilder.DropColumn(
                name: "conference_abstract",
                table: "Conference");

            migrationBuilder.DropColumn(
                name: "conference_link",
                table: "Conference");

            migrationBuilder.DropColumn(
                name: "project_phase_id",
                table: "Conference");

            migrationBuilder.DropColumn(
                name: "group_id",
                table: "AssignReview");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignReview_ProgressReport_progress_report_id",
                table: "AssignReview",
                column: "progress_report_id",
                principalTable: "ProgressReport",
                principalColumn: "ProgressReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompletionRequestDetails_ProjectRequests_request_id",
                table: "CompletionRequestDetails",
                column: "request_id",
                principalTable: "ProjectRequests",
                principalColumn: "request_id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_ProgressReport_ProjectPhase_ProjectPhaseId",
                table: "ProgressReport",
                column: "ProjectPhaseId",
                principalTable: "ProjectPhase",
                principalColumn: "project_phase_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressReport_Projects_ProjectId",
                table: "ProgressReport",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "project_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequests_Fund_Disbursement_fund_disbursement_id",
                table: "ProjectRequests",
                column: "fund_disbursement_id",
                principalTable: "Fund_Disbursement",
                principalColumn: "fund_disbursement_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequests_Groups_assigned_council",
                table: "ProjectRequests",
                column: "assigned_council",
                principalTable: "Groups",
                principalColumn: "group_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequests_ProjectPhase_phase_id",
                table: "ProjectRequests",
                column: "phase_id",
                principalTable: "ProjectPhase",
                principalColumn: "project_phase_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequests_Projects_project_id",
                table: "ProjectRequests",
                column: "project_id",
                principalTable: "Projects",
                principalColumn: "project_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequests_Timeline_timeline_id",
                table: "ProjectRequests",
                column: "timeline_id",
                principalSchema: "dbo",
                principalTable: "Timeline",
                principalColumn: "timeline_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequests_Users_approved_by",
                table: "ProjectRequests",
                column: "approved_by",
                principalTable: "Users",
                principalColumn: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequests_Users_requested_by",
                table: "ProjectRequests",
                column: "requested_by",
                principalTable: "Users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VoteResult_ProgressReport_progress_report_id",
                table: "VoteResult",
                column: "progress_report_id",
                principalTable: "ProgressReport",
                principalColumn: "ProgressReportId");
        }
    }
}
