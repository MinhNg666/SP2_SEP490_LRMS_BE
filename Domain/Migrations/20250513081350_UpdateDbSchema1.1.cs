using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDbSchema11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.AddColumn<int>(
            //     name: "department_id",
            //     table: "Quotas",
            //     type: "int",
            //     nullable: true);

            // migrationBuilder.AddColumn<int>(
            //     name: "num_projects",
            //     table: "Quotas",
            //     type: "int",
            //     nullable: true);

            // migrationBuilder.AddColumn<int>(
            //     name: "quota_year",
            //     table: "Quotas",
            //     type: "int",
            //     nullable: true);

            // migrationBuilder.AddColumn<int>(
            //     name: "project_phase_id",
            //     table: "Project_resources",
            //     type: "int",
            //     nullable: true);

            // migrationBuilder.AddColumn<int>(
            //     name: "notification_type",
            //     table: "Notifications",
            //     type: "int",
            //     nullable: true);

            // migrationBuilder.CreateTable(
            //     name: "AuthorConference",
            //     columns: table => new
            //     {
            //         author_conference_id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         author_id = table.Column<int>(type: "int", nullable: false),
            //         conference_id = table.Column<int>(type: "int", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_AuthorConference", x => x.author_conference_id);
            //         table.ForeignKey(
            //             name: "FK_AuthorConference_Author",
            //             column: x => x.author_id,
            //             principalTable: "Author",
            //             principalColumn: "author_id");
            //         table.ForeignKey(
            //             name: "FK_AuthorConference_Conference",
            //             column: x => x.conference_id,
            //             principalTable: "Conference",
            //             principalColumn: "conference_id");
            //     });

            // migrationBuilder.CreateTable(
            //     name: "AuthorJournal",
            //     columns: table => new
            //     {
            //         author_journal_id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         author_id = table.Column<int>(type: "int", nullable: false),
            //         journal_id = table.Column<int>(type: "int", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_AuthorJournal", x => x.author_journal_id);
            //         table.ForeignKey(
            //             name: "FK_AuthorJournal_Author",
            //             column: x => x.author_id,
            //             principalTable: "Author",
            //             principalColumn: "author_id");
            //         table.ForeignKey(
            //             name: "FK_AuthorJournal_Journal",
            //             column: x => x.journal_id,
            //             principalTable: "Journal",
            //             principalColumn: "journal_id");
            //     });

            // migrationBuilder.CreateTable(
            //     name: "Expertise",
            //     columns: table => new
            //     {
            //         expertise_id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         expertise_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
            //         expertise_status = table.Column<int>(type: "int", nullable: true),
            //         user_id = table.Column<int>(type: "int", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_Expertise", x => x.expertise_id);
            //         table.ForeignKey(
            //             name: "FK_Expertise_User",
            //             column: x => x.user_id,
            //             principalTable: "Users",
            //             principalColumn: "user_id");
            //     });

            // migrationBuilder.CreateTable(
            //     name: "Inspection",
            //     columns: table => new
            //     {
            //         inspection_id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         result = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         result_rating = table.Column<int>(type: "int", nullable: true),
            //         created_at = table.Column<DateTime>(type: "datetime", nullable: true),
            //         updated_at = table.Column<DateTime>(type: "datetime", nullable: true),
            //         project_id = table.Column<int>(type: "int", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_Inspection", x => x.inspection_id);
            //         table.ForeignKey(
            //             name: "FK_Inspection_Project",
            //             column: x => x.project_id,
            //             principalTable: "Projects",
            //             principalColumn: "project_id");
            //     });

            // migrationBuilder.CreateTable(
            //     name: "AssignReview",
            //     columns: table => new
            //     {
            //         assign_id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         scheduled_date = table.Column<DateTime>(type: "datetime", nullable: true),
            //         scheduled_time = table.Column<TimeSpan>(type: "time", nullable: true),
            //         review_type = table.Column<int>(type: "int", nullable: true),
            //         notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         status = table.Column<int>(type: "int", nullable: true),
            //         assigned_by_user_id = table.Column<int>(type: "int", nullable: true),
            //         project_request_id = table.Column<int>(type: "int", nullable: true),
            //         inspection_id = table.Column<int>(type: "int", nullable: true),
            //         fund_disbursement_id = table.Column<int>(type: "int", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_AssignReview", x => x.assign_id);
            //         table.ForeignKey(
            //             name: "FK_AssignReview_FundDisbursement",
            //             column: x => x.fund_disbursement_id,
            //             principalTable: "Fund_Disbursement",
            //             principalColumn: "fund_disbursement_id");
            //         table.ForeignKey(
            //             name: "FK_AssignReview_Inspection_inspection_id",
            //             column: x => x.inspection_id,
            //             principalTable: "Inspection",
            //             principalColumn: "inspection_id");
            //         table.ForeignKey(
            //             name: "FK_AssignReview_ProjectRequest",
            //             column: x => x.project_request_id,
            //             principalTable: "ProjectRequests",
            //             principalColumn: "request_id");
            //         table.ForeignKey(
            //             name: "FK_AssignReview_User_AssignedBy",
            //             column: x => x.assigned_by_user_id,
            //             principalTable: "Users",
            //             principalColumn: "user_id");
            //     });

            // migrationBuilder.CreateTable(
            //     name: "VoteResult",
            //     columns: table => new
            //     {
            //         result_id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         result_status = table.Column<int>(type: "int", nullable: true),
            //         group_id = table.Column<int>(type: "int", nullable: true),
            //         project_request_id = table.Column<int>(type: "int", nullable: true),
            //         inspection_id = table.Column<int>(type: "int", nullable: true),
            //         fund_disbursement_id = table.Column<int>(type: "int", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_VoteResult", x => x.result_id);
            //         table.ForeignKey(
            //             name: "FK_VoteResult_FundDisbursement",
            //             column: x => x.fund_disbursement_id,
            //             principalTable: "Fund_Disbursement",
            //             principalColumn: "fund_disbursement_id");
            //         table.ForeignKey(
            //             name: "FK_VoteResult_Group",
            //             column: x => x.group_id,
            //             principalTable: "Groups",
            //             principalColumn: "group_id");
            //         table.ForeignKey(
            //             name: "FK_VoteResult_Inspection_inspection_id",
            //             column: x => x.inspection_id,
            //             principalTable: "Inspection",
            //             principalColumn: "inspection_id");
            //         table.ForeignKey(
            //             name: "FK_VoteResult_ProjectRequest",
            //             column: x => x.project_request_id,
            //             principalTable: "ProjectRequests",
            //             principalColumn: "request_id");
            //     });

            // migrationBuilder.CreateTable(
            //     name: "CouncilVote",
            //     columns: table => new
            //     {
            //         vote_id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         vote_status = table.Column<int>(type: "int", nullable: true),
            //         comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         project_request_id = table.Column<int>(type: "int", nullable: false),
            //         group_member_id = table.Column<int>(type: "int", nullable: false),
            //         group_id = table.Column<int>(type: "int", nullable: false),
            //         fund_disbursement_id = table.Column<int>(type: "int", nullable: true),
            //         inspection_id = table.Column<int>(type: "int", nullable: true),
            //         vote_result_id = table.Column<int>(type: "int", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_CouncilVote", x => x.vote_id);
            //         table.ForeignKey(
            //             name: "FK_CouncilVote_FundDisbursement",
            //             column: x => x.fund_disbursement_id,
            //             principalTable: "Fund_Disbursement",
            //             principalColumn: "fund_disbursement_id");
            //         table.ForeignKey(
            //             name: "FK_CouncilVote_Group",
            //             column: x => x.group_id,
            //             principalTable: "Groups",
            //             principalColumn: "group_id");
            //         table.ForeignKey(
            //             name: "FK_CouncilVote_GroupMember",
            //             column: x => x.group_member_id,
            //             principalTable: "Group_Member",
            //             principalColumn: "group_member_id");
            //         table.ForeignKey(
            //             name: "FK_CouncilVote_Inspection_inspection_id",
            //             column: x => x.inspection_id,
            //             principalTable: "Inspection",
            //             principalColumn: "inspection_id");
            //         table.ForeignKey(
            //             name: "FK_CouncilVote_ProjectRequest",
            //             column: x => x.project_request_id,
            //             principalTable: "ProjectRequests",
            //             principalColumn: "request_id");
            //         table.ForeignKey(
            //             name: "FK_CouncilVote_VoteResult_vote_result_id",
            //             column: x => x.vote_result_id,
            //             principalTable: "VoteResult",
            //             principalColumn: "result_id",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            // migrationBuilder.CreateIndex(
            //     name: "IX_Quotas_department_id",
            //     table: "Quotas",
            //     column: "department_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_Project_resources_project_phase_id",
            //     table: "Project_resources",
            //     column: "project_phase_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_AssignReview_assigned_by_user_id",
            //     table: "AssignReview",
            //     column: "assigned_by_user_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_AssignReview_fund_disbursement_id",
            //     table: "AssignReview",
            //     column: "fund_disbursement_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_AssignReview_inspection_id",
            //     table: "AssignReview",
            //     column: "inspection_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_AssignReview_project_request_id",
            //     table: "AssignReview",
            //     column: "project_request_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_AuthorConference_author_id",
            //     table: "AuthorConference",
            //     column: "author_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_AuthorConference_conference_id",
            //     table: "AuthorConference",
            //     column: "conference_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_AuthorJournal_author_id",
            //     table: "AuthorJournal",
            //     column: "author_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_AuthorJournal_journal_id",
            //     table: "AuthorJournal",
            //     column: "journal_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_CouncilVote_fund_disbursement_id",
            //     table: "CouncilVote",
            //     column: "fund_disbursement_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_CouncilVote_group_id",
            //     table: "CouncilVote",
            //     column: "group_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_CouncilVote_group_member_id",
            //     table: "CouncilVote",
            //     column: "group_member_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_CouncilVote_inspection_id",
            //     table: "CouncilVote",
            //     column: "inspection_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_CouncilVote_project_request_id",
            //     table: "CouncilVote",
            //     column: "project_request_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_CouncilVote_vote_result_id",
            //     table: "CouncilVote",
            //     column: "vote_result_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_Expertise_user_id",
            //     table: "Expertise",
            //     column: "user_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_Inspection_project_id",
            //     table: "Inspection",
            //     column: "project_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_VoteResult_fund_disbursement_id",
            //     table: "VoteResult",
            //     column: "fund_disbursement_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_VoteResult_group_id",
            //     table: "VoteResult",
            //     column: "group_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_VoteResult_inspection_id",
            //     table: "VoteResult",
            //     column: "inspection_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_VoteResult_project_request_id",
            //     table: "VoteResult",
            //     column: "project_request_id");

            // migrationBuilder.AddForeignKey(
            //     name: "FK_ProjectResource_ProjectPhase",
            //     table: "Project_resources",
            //     column: "project_phase_id",
            //     principalTable: "ProjectPhase",
            //     principalColumn: "project_phase_id");

            // migrationBuilder.AddForeignKey(
            //     name: "FK_Quota_Department",
            //     table: "Quotas",
            //     column: "department_id",
            //     principalTable: "Department",
            //     principalColumn: "department_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectResource_ProjectPhase",
                table: "Project_resources");

            // migrationBuilder.DropForeignKey(
            //     name: "FK_Quota_Department",
            //     table: "Quotas");

            // migrationBuilder.DropTable(
            //     name: "AssignReview");

            // migrationBuilder.DropTable(
            //     name: "AuthorConference");

            // migrationBuilder.DropTable(
            //     name: "AuthorJournal");

            // migrationBuilder.DropTable(
            //     name: "CouncilVote");

            // migrationBuilder.DropTable(
            //     name: "Expertise");

            // migrationBuilder.DropTable(
            //     name: "VoteResult");

            // migrationBuilder.DropTable(
            //     name: "Inspection");

            // migrationBuilder.DropIndex(
            //     name: "IX_Quotas_department_id",
            //     table: "Quotas");

            // migrationBuilder.DropIndex(
            //     name: "IX_Project_resources_project_phase_id",
            //     table: "Project_resources");

            // migrationBuilder.DropColumn(
            //     name: "department_id",
            //     table: "Quotas");

            // migrationBuilder.DropColumn(
            //     name: "num_projects",
            //     table: "Quotas");

            // migrationBuilder.DropColumn(
            //     name: "quota_year",
            //     table: "Quotas");

            // migrationBuilder.DropColumn(
            //     name: "project_phase_id",
            //     table: "Project_resources");

            // migrationBuilder.DropColumn(
            //     name: "notification_type",
            //     table: "Notifications");
        }
    }
}
