using Microsoft.EntityFrameworkCore;
using API.Model.DB;
using System;

namespace API.Model
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ProductCategory (Many-to-Many)
            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Product)
                .WithMany(p => p.ProductCategories)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Category)
                .WithMany(c => c.ProductCategories)
                .HasForeignKey(pc => pc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductCategory>()
                .HasIndex(pc => new { pc.ProductId, pc.CategoryId })
                .IsUnique();

            // Product - Supplier
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order - Customer
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderItem - Order & Product
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review - Product & Customer
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.ProductId, r.CustomerId })
                .IsUnique();

            // Indexes
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.NamaProduct);
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.IsActive);
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderDate);

            // Decimal precision
            modelBuilder.Entity<Product>()
                .Property(p => p.Harga)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Order>()
                .Property(o => o.TaxAmount)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingCost)
                .HasPrecision(18, 2);
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.TotalPrice)
                .HasPrecision(18, 2);

            // Seed data (gunakan tanggal fix, bukan DateTime.Now)
            var fixedDate = new DateTime(2025, 1, 1);

            modelBuilder.Entity<Supplier>().HasData(
                new Supplier { Id = 1, CompanyName = "Tech Supplies Co.", ContactPerson = "John Doe", Email = "john@techsupplies.com", Phone = "+1234567890", Address = "123 Tech Street", City = "Tech City", CreatedAt = fixedDate, UpdatedAt = fixedDate },
                new Supplier { Id = 2, CompanyName = "Fashion Hub Ltd.", ContactPerson = "Jane Smith", Email = "jane@fashionhub.com", Phone = "+0987654321", Address = "456 Fashion Ave", City = "Style City", CreatedAt = fixedDate, UpdatedAt = fixedDate }
            );

            modelBuilder.Entity<Customer>().HasData(
                new Customer { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice@example.com", Phone = "+1111111111", Address = "789 Customer St", City = "Customer City", PostalCode = "12345", CreatedAt = fixedDate, UpdatedAt = fixedDate },
                new Customer { Id = 2, FirstName = "Bob", LastName = "Wilson", Email = "bob@example.com", Phone = "+2222222222", Address = "321 Buyer Blvd", City = "Buyer City", PostalCode = "54321", CreatedAt = fixedDate, UpdatedAt = fixedDate }
            );
        }
    }
}
