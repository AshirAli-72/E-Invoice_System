using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Invoice_system.Data.Migrations
{
    public partial class create_app_settings_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_settings", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_settings");
        }
    }
}
