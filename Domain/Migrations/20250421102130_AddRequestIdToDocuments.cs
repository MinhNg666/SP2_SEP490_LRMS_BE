using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestIdToDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequestId",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_RequestId",
                table: "Documents",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_ProjectRequests_RequestId",
                table: "Documents",
                column: "RequestId",
                principalTable: "ProjectRequests",
                principalColumn: "request_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_ProjectRequests_RequestId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_RequestId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "Documents");
        }
    }
}
