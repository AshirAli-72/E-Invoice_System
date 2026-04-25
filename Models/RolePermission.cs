using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Invoice_system.Models
{
    [Table("roles_permissions")]
    public class RolePermission
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("role_id")]
        public int RoleId { get; set; }

        [Column("dashboard")]
        public bool Dashboard { get; set; }

        [Column("customers")]
        public bool Customers { get; set; }

        [Column("products")]
        public bool Products { get; set; }

        [Column("sales")]
        public bool Sales { get; set; }

        [Column("invoices")]
        public bool Invoices { get; set; }

        [Column("employees")]
        public bool Employees { get; set; }

        [Column("Reports")]
        public bool Reports { get; set; }

        [Column("settings")]
        public bool Settings { get; set; }

        [Column("customer_report")]
        public bool CustomerReport { get; set; }

        [Column("sale_report")]
        public bool SaleReport { get; set; }

        [Column("product_report")]
        public bool ProductReport { get; set; }

        [Column("invoice_report")]
        public bool InvoiceReport { get; set; }

        [Column("employee_report")]
        public bool EmployeeReport { get; set; }

        [Column("returns_report")]
        public bool ReturnsReport { get; set; }

        [Column("daily_summary")]
        public bool DailySummary { get; set; }

        [Column("inventory")]
        public bool Inventory { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }
    }
}
