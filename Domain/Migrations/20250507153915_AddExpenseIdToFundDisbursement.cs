using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseIdToFundDisbursement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.AddColumn<int>(
            //     name: "ExpenseId",
            //     table: "Fund_Disbursement",
            //     type: "int",
            //     nullable: true);

            // migrationBuilder.CreateIndex(
            //     name: "IX_Fund_Disbursement_ExpenseId",
            //     table: "Fund_Disbursement",
            //     column: "ExpenseId",
            //     unique: true,
            //     filter: "[ExpenseId] IS NOT NULL");

            // migrationBuilder.AddForeignKey(
            //     name: "FK_FundDisbursement_ConferenceExpense",
            //     table: "Fund_Disbursement",
            //     column: "ExpenseId",
            //     principalTable: "Conference_expense",
            //     principalColumn: "expense_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropForeignKey(
            //     name: "FK_FundDisbursement_ConferenceExpense",
            //     table: "Fund_Disbursement");

            // migrationBuilder.DropIndex(
            //     name: "IX_Fund_Disbursement_ExpenseId",
            //     table: "Fund_Disbursement");

            // migrationBuilder.DropColumn(
            //     name: "ExpenseId",
            //     table: "Fund_Disbursement");
        }
    }
}
