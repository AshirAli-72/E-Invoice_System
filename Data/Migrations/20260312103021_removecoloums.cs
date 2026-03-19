using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Invoice_system.Data.Migrations
{
    /// <inheritdoc />
    public partial class removecoloums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropColumn(
            //     name: "customer_name",
            //     table: "sale_details");

            // migrationBuilder.DropColumn(
            //     name: "customer_name",
            //     table: "return_details");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "customer_name",
                table: "sale_details",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "customer_name",
                table: "return_details",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
