using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddConferenceJournalAndTypeToFundDisbursement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.AddColumn<int>(
            //     name: "ConferenceId",
            //     table: "Fund_Disbursement",
            //     type: "int",
            //     nullable: true);

            // migrationBuilder.AddColumn<int>(
            //     name: "FundDisbursementType",
            //     table: "Fund_Disbursement",
            //     type: "int",
            //     nullable: true);

            // migrationBuilder.AddColumn<int>(
            //     name: "JournalId",
            //     table: "Fund_Disbursement",
            //     type: "int",
            //     nullable: true);

            // migrationBuilder.CreateIndex(
            //     name: "IX_Fund_Disbursement_ConferenceId",
            //     table: "Fund_Disbursement",
            //     column: "ConferenceId");

            // migrationBuilder.CreateIndex(
            //     name: "IX_Fund_Disbursement_JournalId",
            //     table: "Fund_Disbursement",
            //     column: "JournalId");

            // migrationBuilder.AddForeignKey(
            //     name: "FK_Fund_Disbursement_Conference_ConferenceId",
            //     table: "Fund_Disbursement",
            //     column: "ConferenceId",
            //     principalTable: "Conference",
            //     principalColumn: "conference_id");

            // migrationBuilder.AddForeignKey(
            //     name: "FK_Fund_Disbursement_Journal_JournalId",
            //     table: "Fund_Disbursement",
            //     column: "JournalId",
            //     principalTable: "Journal",
            //     principalColumn: "journal_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropForeignKey(
            //     name: "FK_Fund_Disbursement_Conference_ConferenceId",
            //     table: "Fund_Disbursement");

            // migrationBuilder.DropForeignKey(
            //     name: "FK_Fund_Disbursement_Journal_JournalId",
            //     table: "Fund_Disbursement");

            // migrationBuilder.DropIndex(
            //     name: "IX_Fund_Disbursement_ConferenceId",
            //     table: "Fund_Disbursement");

            // migrationBuilder.DropIndex(
            //     name: "IX_Fund_Disbursement_JournalId",
            //     table: "Fund_Disbursement");

            // migrationBuilder.DropColumn(
            //     name: "ConferenceId",
            //     table: "Fund_Disbursement");

            // migrationBuilder.DropColumn(
            //     name: "FundDisbursementType",
            //     table: "Fund_Disbursement");

            // migrationBuilder.DropColumn(
            //     name: "JournalId",
            //     table: "Fund_Disbursement");
        }
    }
}
