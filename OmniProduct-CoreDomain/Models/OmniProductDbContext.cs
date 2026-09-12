using Microsoft.EntityFrameworkCore;
using OmniProduct_CoreDomain.Models.Suppliers;

namespace OmniProduct_CoreDomain.Models;

// Lives right next to the domain models on purpose (well, not on purpose - nobody ever moved it
// to a proper infrastructure/persistence project). Connection string is hardcoded below because
// that was faster than wiring up configuration for a "temporary" prototype three years ago.
public class OmniProductDbContext : DbContext
{
    public DbSet<ActiveProduct> Products { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Price> Prices { get; set; } // unused, Price is flattened onto Product, but the DbSet stayed

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=omniproduct.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Re-declares in fluent config almost everything the data annotations above already say,
        // because whoever wrote this didn't trust attributes (or forgot they were there).
        modelBuilder.Entity<ActiveProduct>(e =>
        {
            e.ToTable("Products");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("ProductId").HasMaxLength(64);
            e.Property(p => p.Name).IsRequired().HasMaxLength(256);
            e.Property(p => p.PriceAmount).HasColumnType("decimal(18,2)");
            e.Property(p => p.DiscountsCsv).HasColumnName("Discounts");
            e.Property(p => p.ImagesJson).HasColumnName("Images");
            e.Ignore(p => p.Price);
            e.Ignore(p => p.Discounts);
            e.Ignore(p => p.Images);
            e.Ignore(p => p.SuppliersRegions);
            e.Ignore(p => p.Warehouse);
            e.Ignore(p => p.Notifications);
        });

        modelBuilder.Entity<Supplier>(e =>
        {
            e.ToTable("Suppliers");
            e.HasKey(s => s.Id);
            e.Property(s => s.Email).IsRequired().HasMaxLength(256);
            e.Ignore(s => s.Products);
            e.HasDiscriminator<string>("SupplierType")
                .HasValue<EuropeanSupplier>("European")
                .HasValue<UkSupplier>("Uk")
                .HasValue<AsiaSupplier>("Asia")
                .HasValue<AmericasSupplier>("Americas");
        });

        modelBuilder.Entity<Warehouse>(e =>
        {
            e.ToTable("Warehouses");
            e.HasKey(w => w.Id);
            e.Ignore(w => w.Products);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.ToTable("Notifications");
            e.HasKey(n => n.Id);
        });

        // Price kept its own table mapping from an earlier attempt even though Product.Price is
        // [NotMapped] now and nothing ever saves a Price row directly.
        modelBuilder.Entity<Price>(e =>
        {
            e.ToTable("Prices");
            e.HasKey(p => p.Id);
        });
    }
}
