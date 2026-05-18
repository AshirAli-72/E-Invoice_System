using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Invoice_system.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "brands",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    brand_title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_brands", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "currencies",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    symbol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    exchange_rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currencies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    contact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "employee",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    date = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    emp_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cnic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mobile_no = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    image_path = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    invoice_no = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    seller_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    seller_contact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    seller_address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    customer_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    customer_address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    customer_contact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    prod_nameservice = table.Column<string>(name: "prod_name/service", type: "nvarchar(max)", nullable: true),
                    qtyunit_type = table.Column<string>(name: "qty/unit_type", type: "nvarchar(max)", nullable: true),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    total_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    payment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products_services",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    prod_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    barcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    manufacture_date = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    expiry_date = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                });

            migrationBuilder.CreateTable(
                name: "return_details",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    billNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sale_id = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    no_of_items = table.Column<int>(type: "int", nullable: false),
                    qty = table.Column<int>(type: "int", nullable: false),
                    total_qty = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    method = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_return_details", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sale_details",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    billNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    no_of_items = table.Column<int>(type: "int", nullable: false),
                    qty = table.Column<int>(type: "int", nullable: false),
                    total_qty = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    expiry_date = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    total_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    payment_method = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_returned = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sale_details", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stock_details",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    item_barcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    pur_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    sale_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    whole_sale_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    stock_alert = table.Column<int>(type: "int", nullable: false),
                    date_of_manafacture = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date_of_expiry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    total_pur_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_details", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stock_history",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    date = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_quantity = table.Column<int>(type: "int", nullable: false),
                    old_quantity = table.Column<int>(type: "int", nullable: false),
                    new_purchase_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    old_purchase_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    new_sale_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    old_sale_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    product_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "store_configurations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    shop_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    owner_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    business_nature = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    branch = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    shop_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    phone_1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    phone_2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    logo_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    comments = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_store_configurations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles_permissions",
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
                    employees = table.Column<bool>(type: "bit", nullable: false),
                    Reports = table.Column<bool>(type: "bit", nullable: false),
                    settings = table.Column<bool>(type: "bit", nullable: false),
                    customer_report = table.Column<bool>(type: "bit", nullable: false),
                    sale_report = table.Column<bool>(type: "bit", nullable: false),
                    product_report = table.Column<bool>(type: "bit", nullable: false),
                    invoice_report = table.Column<bool>(type: "bit", nullable: false),
                    employee_report = table.Column<bool>(type: "bit", nullable: false),
                    returns_report = table.Column<bool>(type: "bit", nullable: false),
                    daily_summary = table.Column<bool>(type: "bit", nullable: false),
                    inventory = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles_permissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_roles_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    emp_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_employee_emp_id",
                        column: x => x.emp_id,
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "employee",
                columns: new[] { "id", "address", "cnic", "date", "email", "emp_code", "full_name", "image_path", "mobile_no", "salary", "status" },
                values: new object[] { 1, "Admin Address", "00000-0000000-0", "1-1-2024", "admin@pos.com", "EMP-001", "Admin", null, "0000-0000000", 0m, "Active" });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "role_title" },
                values: new object[] { 1, "Admin" });

            migrationBuilder.InsertData(
                table: "roles_permissions",
                columns: new[] { "id", "customer_report", "customers", "daily_summary", "dashboard", "employee_report", "employees", "inventory", "invoice_report", "invoices", "product_report", "products", "Reports", "returns_report", "role_id", "sale_report", "sales", "settings" },
                values: new object[] { 1, true, true, true, true, true, true, true, true, true, true, true, true, true, 1, true, true, true });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "email", "emp_id", "password", "role_id", "status", "username" },
                values: new object[] { 1, "admin@pos.com", 1, "admin123", 1, "Active", "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_roles_permissions_role_id",
                table: "roles_permissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_emp_id",
                table: "users",
                column: "emp_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_role_id",
                table: "users",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "brands");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "currencies");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "products_services");

            migrationBuilder.DropTable(
                name: "return_details");

            migrationBuilder.DropTable(
                name: "roles_permissions");

            migrationBuilder.DropTable(
                name: "sale_details");

            migrationBuilder.DropTable(
                name: "stock_details");

            migrationBuilder.DropTable(
                name: "stock_history");

            migrationBuilder.DropTable(
                name: "store_configurations");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "employee");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
