using DemoProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace DemoProject.Infrastructure.Data
{
    public class DemoProjectContext : DbContext
    {
        public DemoProjectContext(DbContextOptions<DemoProjectContext> options) : base(options)
        {
        }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure AuditLog entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders"); // Map to Invoices table

                entity.HasKey(e => e.Id); // Map to Orders table

                entity.Property(e => e.Name).HasMaxLength(250);

            });

            // Configure User entity
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.ToTable("Invoices"); // Map to Invoices table

                entity.HasKey(e => e.Id); // Primary Key

            });
        }
    }
}
