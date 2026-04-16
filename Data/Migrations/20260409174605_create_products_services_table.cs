    using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Invoice_system.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260409174605_create_products_services_table")]
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
                    prod_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    barcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    manufacture_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    expiry_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    prod_state = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    item_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    size = table.Column<int>(type: "int", nullable: true),
                    pic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    brand_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products_services", x => x.id);
                    table.ForeignKey("FK_products_services_categories_category_id",
                        x => x.category_id,
                        "categories",
                        "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey("FK_products_services_brands_brand_id",
                        x => x.brand_id,
                        "brands",
                        "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_products_services_category_id",
                table: "products_services",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_services_brand_id",
                table: "products_services",
                column: "brand_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "products_services");
        }
    }
}
