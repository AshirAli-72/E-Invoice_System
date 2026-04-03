using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Invoice_system.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "billNo",
                table: "return_details",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_return_details_billNo",
                table: "return_details",
                column: "billNo");

            migrationBuilder.CreateIndex(
                name: "IX_return_details_date",
                table: "return_details",
                column: "date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_return_details_billNo",
                table: "return_details");

            migrationBuilder.DropIndex(
                name: "IX_return_details_date",
                table: "return_details");

            migrationBuilder.AlterColumn<string>(
                name: "billNo",
                table: "return_details",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
