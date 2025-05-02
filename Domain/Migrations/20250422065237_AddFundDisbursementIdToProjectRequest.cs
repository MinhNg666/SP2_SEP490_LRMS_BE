using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddFundDisbursementIdToProjectRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "fund_disbursement_id",
                table: "ProjectRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequests_fund_disbursement_id",
                table: "ProjectRequests",
                column: "fund_disbursement_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequests_Fund_Disbursement_fund_disbursement_id",
                table: "ProjectRequests",
                column: "fund_disbursement_id",
                principalTable: "Fund_Disbursement",
                principalColumn: "fund_disbursement_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequests_Fund_Disbursement_fund_disbursement_id",
                table: "ProjectRequests");

            migrationBuilder.DropIndex(
                name: "IX_ProjectRequests_fund_disbursement_id",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "fund_disbursement_id",
                table: "ProjectRequests");
        }
    }
}
