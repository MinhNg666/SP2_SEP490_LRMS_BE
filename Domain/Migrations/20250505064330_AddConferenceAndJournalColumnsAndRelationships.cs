using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddConferenceAndJournalColumnsAndRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_ProjectRequests_RequestId",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "dbo",
                table: "TimelineSequence",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "dbo",
                table: "Timeline",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "RequestId",
                table: "Documents",
                newName: "request_id");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_RequestId",
                table: "Documents",
                newName: "IX_Documents_request_id");

            migrationBuilder.AddColumn<decimal>(
                name: "journal_funding",
                table: "Journal",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "journal_status",
                table: "Journal",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "conference_id",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "journal_id",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "conference_funding",
                table: "Conference",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "conference_status",
                table: "Conference",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "conference_submission_status",
                table: "Conference",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reviewer_comment",
                table: "Conference",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_conference_id",
                table: "Documents",
                column: "conference_id");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_journal_id",
                table: "Documents",
                column: "journal_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Conference",
                table: "Documents",
                column: "conference_id",
                principalTable: "Conference",
                principalColumn: "conference_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Journal",
                table: "Documents",
                column: "journal_id",
                principalTable: "Journal",
                principalColumn: "journal_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_ProjectRequests",
                table: "Documents",
                column: "request_id",
                principalTable: "ProjectRequests",
                principalColumn: "request_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Conference",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Journal",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_ProjectRequests",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_conference_id",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_journal_id",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "journal_funding",
                table: "Journal");

            migrationBuilder.DropColumn(
                name: "journal_status",
                table: "Journal");

            migrationBuilder.DropColumn(
                name: "conference_id",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "journal_id",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "conference_funding",
                table: "Conference");

            migrationBuilder.DropColumn(
                name: "conference_status",
                table: "Conference");

            migrationBuilder.DropColumn(
                name: "conference_submission_status",
                table: "Conference");

            migrationBuilder.DropColumn(
                name: "reviewer_comment",
                table: "Conference");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "dbo",
                table: "TimelineSequence",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "dbo",
                table: "Timeline",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "request_id",
                table: "Documents",
                newName: "RequestId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_request_id",
                table: "Documents",
                newName: "IX_Documents_RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_ProjectRequests_RequestId",
                table: "Documents",
                column: "RequestId",
                principalTable: "ProjectRequests",
                principalColumn: "request_id");
        }
    }
}
