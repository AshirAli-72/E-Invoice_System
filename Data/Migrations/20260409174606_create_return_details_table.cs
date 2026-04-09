using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Invoice_system.Data.Migrations
{
    public partial class create_return_details_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "return_details",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    billNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sale_id = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    prod_nameservice = table.Column<string>(name: "prod_name/service", type: "nvarchar(max)", nullable: true),
                    barcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    qtyunit_type = table.Column<string>(name: "qty/unit_type", type: "nvarchar(max)", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    method = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_return_details", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "return_details");
        }
    }
}
