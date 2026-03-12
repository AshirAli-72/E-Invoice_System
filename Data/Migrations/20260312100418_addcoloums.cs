using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Invoice_system.Data.Migrations
{
    /// <inheritdoc />
    public partial class addcoloums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("TRUNCATE TABLE sale_details; TRUNCATE TABLE return_details;");


            migrationBuilder.AddColumn<bool>(
                name: "is_returned",
                table: "sale_details",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_returned",
                table: "sale_details");
        }
    }
}
