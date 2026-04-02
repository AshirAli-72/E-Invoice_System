using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Invoice_system.Data.Migrations
{
    /// <inheritdoc />
    public partial class ApplyMissingPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "users",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "billNo",
                table: "sale_details",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "prod_name/service",
                table: "products_services",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "barcode",
                table: "products_services",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "invoices",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "invoice_no",
                table: "invoices",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "customers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true,
                filter: "[email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_sale_details_billNo",
                table: "sale_details",
                column: "billNo");

            migrationBuilder.CreateIndex(
                name: "IX_sale_details_date",
                table: "sale_details",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "IX_sale_details_total_price",
                table: "sale_details",
                column: "total_price");

            migrationBuilder.CreateIndex(
                name: "IX_products_services_barcode",
                table: "products_services",
                column: "barcode");

            migrationBuilder.CreateIndex(
                name: "IX_products_services_prod_name/service",
                table: "products_services",
                column: "prod_name/service");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_date",
                table: "invoices",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_invoice_no",
                table: "invoices",
                column: "invoice_no");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_status",
                table: "invoices",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_customers_name",
                table: "customers",
                column: "name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_sale_details_billNo",
                table: "sale_details");

            migrationBuilder.DropIndex(
                name: "IX_sale_details_date",
                table: "sale_details");

            migrationBuilder.DropIndex(
                name: "IX_sale_details_total_price",
                table: "sale_details");

            migrationBuilder.DropIndex(
                name: "IX_products_services_barcode",
                table: "products_services");

            migrationBuilder.DropIndex(
                name: "IX_products_services_prod_name/service",
                table: "products_services");

            migrationBuilder.DropIndex(
                name: "IX_invoices_date",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_invoices_invoice_no",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_invoices_status",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_customers_name",
                table: "customers");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "billNo",
                table: "sale_details",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "prod_name/service",
                table: "products_services",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "barcode",
                table: "products_services",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "invoices",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "invoice_no",
                table: "invoices",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "customers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
