using Microsoft.EntityFrameworkCore;
using E_Invoice_system.Models;

namespace E_Invoice_system.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

 
        public DbSet<customers> customers { get; set; }
        public DbSet<ProductService> products_services { get; set; }
        public DbSet<users> users { get; set; }
        public DbSet<invoices> invoices { get; set; }
       
        public DbSet<Sale> sales { get; set; }
        public DbSet<ReturnDetail> returns { get; set; }

        public DbSet<Currency> currencies { get; set; }
        public DbSet<Role> roles { get; set; }
        public DbSet<RolePermission> roles_permissions { get; set; }
        public DbSet<StoreConfiguration> store_configurations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductService>()
                .Property(p => p.price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ProductService>()
                .Property(p => p.discount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ProductService>()
                .Property(p => p.tax)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Sale>()
                .Property(s => s.price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Sale>()
                .Property(s => s.discount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Sale>()
                .Property(s => s.total_price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<invoices>()
                .Property(i => i.price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<invoices>()
                .Property(i => i.discount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<invoices>()
                .Property(i => i.total_price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ReturnDetail>()
                .Property(r => r.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Currency>()
                .Property(c => c.exchange_rate)
                .HasColumnType("decimal(18,2)");
        }
    }
}
