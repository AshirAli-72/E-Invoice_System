using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Invoice_system.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionTypeToSale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropTable(name: "payment");

            migrationBuilder.AlterColumn<DateTime>(
                name: "expiry_date",
                table: "sale_details",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "transaction_type",
                table: "sale_details",
                type: "nvarchar(max)",
                nullable: true);
/*
            migrationBuilder.AddColumn<DateTime>(
                name: "date",
                table: "products_services",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "expiry_date",
                table: "products_services",
                type: "datetime2",
                nullable: true);
*/
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "transaction_type",
                table: "sale_details");

            migrationBuilder.DropColumn(
                name: "date",
                table: "products_services");

            migrationBuilder.DropColumn(
                name: "expiry_date",
                table: "products_services");

            migrationBuilder.AlterColumn<DateTime>(
                name: "expiry_date",
                table: "sale_details",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "payment",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Payment_method = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment", x => x.id);
                });
        }
    }
}
