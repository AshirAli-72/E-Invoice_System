using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Invoice_system.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditLimitToCustomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "date",
                table: "sale_details",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "sale_id",
                table: "return_details",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AddColumn<decimal>(
                name: "balance",
                table: "customers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "credit_limit",
                table: "customers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "balance",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "credit_limit",
                table: "customers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date",
                table: "sale_details",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "sale_id",
                table: "return_details",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 2);
        }
    }
}
