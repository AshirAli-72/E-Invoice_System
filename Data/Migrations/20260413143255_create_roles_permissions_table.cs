using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Invoice_system.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260413143255_create_roles_permissions_table")]
    /// <inheritdoc />
    public partial class create_roles_permissions_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF OBJECT_ID('create_roles_permissions_table', 'U') IS NOT NULL DROP TABLE create_roles_permissions_table;");
            migrationBuilder.CreateTable(
                name: "create_roles_permissions_table",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    dashboard = table.Column<bool>(type: "bit", nullable: false),
                    customers = table.Column<bool>(type: "bit", nullable: false),
                    products = table.Column<bool>(type: "bit", nullable: false),
                    sales = table.Column<bool>(type: "bit", nullable: false),
                    invoices = table.Column<bool>(type: "bit", nullable: false),
                    settings = table.Column<bool>(type: "bit", nullable: false),
                    customer_report = table.Column<bool>(type: "bit", nullable: false),
                    sale_report = table.Column<bool>(type: "bit", nullable: false),
                    product_report = table.Column<bool>(type: "bit", nullable: false),
                    invoice_report = table.Column<bool>(type: "bit", nullable: false),
                    returns_report = table.Column<bool>(type: "bit", nullable: false),
                    daily_summary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_create_roles_permissions_table", x => x.id);
                    table.ForeignKey(
                        name: "FK_create_roles_permissions_table_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_create_roles_permissions_table_role_id",
                table: "create_roles_permissions_table",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "create_roles_permissions_table");
        }
    }
}
