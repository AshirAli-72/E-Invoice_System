using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Invoice_system.Data.Migrations
{
    public partial class create_products_services_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "products_services",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    prod_nameservice = table.Column<string>(name: "prod_name/service", type: "nvarchar(max)", nullable: true),
                    barcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    qtyunit_type = table.Column<string>(name: "qty/unit_type", type: "nvarchar(max)", nullable: true),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products_services", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "products_services");
        }
    }
}
