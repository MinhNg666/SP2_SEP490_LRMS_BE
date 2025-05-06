using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestTrackingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.CreateTable(
            //     name: "ProjectRequests",
            //     columns: table => new
            //     {
            //         request_id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         project_id = table.Column<int>(type: "int", nullable: false),
            //         phase_id = table.Column<int>(type: "int", nullable: true),
            //         timeline_id = table.Column<int>(type: "int", nullable: true),
            //         request_type = table.Column<int>(type: "int", nullable: false),
            //         requested_by = table.Column<int>(type: "int", nullable: false),
            //         requested_at = table.Column<DateTime>(type: "datetime2", nullable: false),
            //         assigned_council = table.Column<int>(type: "int", nullable: true),
            //         approval_status = table.Column<int>(type: "int", nullable: true),
            //         approved_by = table.Column<int>(type: "int", nullable: true),
            //         approved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
            //         rejection_reason = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_ProjectRequests", x => x.request_id);
            //         table.ForeignKey(
            //             name: "FK_ProjectRequests_Groups_assigned_council",
            //             column: x => x.assigned_council,
            //             principalTable: "Groups",
            //             principalColumn: "group_id");
            //         table.ForeignKey(
            //             name: "FK_ProjectRequests_ProjectPhase_phase_id",
            //             column: x => x.phase_id,
            //             principalTable: "ProjectPhase",
            //             principalColumn: "project_phase_id");
            //         table.ForeignKey(
            //             name: "FK_ProjectRequests_Projects_project_id",
            //             column: x => x.project_id,
            //             principalTable: "Projects",
            //             principalColumn: "project_id",
            //             onDelete: ReferentialAction.Cascade);
            //         table.ForeignKey(
            //             name: "FK_ProjectRequests_Timeline_timeline_id",
            //             column: x => x.timeline_id,
            //             principalSchema: "dbo",
            //             principalTable: "Timeline",
            //             principalColumn: "timeline_id");
            //         table.ForeignKey(
            //             name: "FK_ProjectRequests_Users_approved_by",
            //             column: x => x.approved_by,
            //             principalTable: "Users",
            //             principalColumn: "user_id");
            //         table.ForeignKey(
            //             name: "FK_ProjectRequests_Users_requested_by",
            //             column: x => x.requested_by,
            //             principalTable: "Users",
            //             principalColumn: "user_id",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            // migrationBuilder.CreateTable(
            //     name: "CompletionRequestDetails",
            //     columns: table => new
            //     {
            //         completion_detail_id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         request_id = table.Column<int>(type: "int", nullable: false),
            //         budget_remaining = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            //         budget_reconciled = table.Column<bool>(type: "bit", nullable: false),
            //         completion_summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //         budget_variance_explanation = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_CompletionRequestDetails", x => x.completion_detail_id);
            //         table.ForeignKey(
            //             name: "FK_CompletionRequestDetails_ProjectRequests_request_id",
            //             column: x => x.request_id,
            //             principalTable: "ProjectRequests",
            //             principalColumn: "request_id",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            migrationBuilder.CreateIndex(
                name: "IX_CompletionRequestDetails_request_id",
                table: "CompletionRequestDetails",
                column: "request_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequests_approved_by",
                table: "ProjectRequests",
                column: "approved_by");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequests_assigned_council",
                table: "ProjectRequests",
                column: "assigned_council");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequests_phase_id",
                table: "ProjectRequests",
                column: "phase_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequests_project_id",
                table: "ProjectRequests",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequests_requested_by",
                table: "ProjectRequests",
                column: "requested_by");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequests_timeline_id",
                table: "ProjectRequests",
                column: "timeline_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // We also need to comment out the DropTable calls here
            // if the Up method didn't create them, the Down shouldn't drop them.
            // migrationBuilder.DropTable(
            //     name: "CompletionRequestDetails");

            // migrationBuilder.DropTable(
            //     name: "ProjectRequests");
        }
    }
}
