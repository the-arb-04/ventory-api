using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Inventory_Tracker.Models;


namespace Inventory_Tracker.Models
{
    // 1. Change the base class from DbContext to IdentityDbContext<IdentityUser>
   public partial class InventoryDbContext 
    : IdentityDbContext<ApplicationUser, IdentityRole, string>

    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Item> Items { get; set; } = null!;
        public virtual DbSet<ItemHistory> ItemHistories { get; set; } = null!;
        public virtual DbSet<Sale> Sales { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public virtual DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public virtual DbSet<AiSummary> AiSummaries { get; set; }

        // 2. The OnConfiguring method with the hardcoded connection string is removed.
        //    The connection string is now properly managed through Program.cs and dependency injection.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Your existing, scaffolded model configurations are preserved below.

            // --------- Category Table Mapping ----------
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("categories_pkey");

                entity.ToTable("categories");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");
            });

            // --------- Items Table Mapping ----------
            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("items_pkey");

                entity.ToTable("items");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
                entity.Property(e => e.Sku).HasColumnName("sku");
                entity.Property(e => e.Barcode).HasColumnName("barcode");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Brand).HasColumnName("brand");
                entity.Property(e => e.SupplierId).HasColumnName("supplier_id");

                entity.Property(e => e.SellingPrice).HasPrecision(10, 2).HasColumnName("sellingprice");
                entity.Property(e => e.CostPrice).HasPrecision(10, 2).HasColumnName("costprice");
                entity.Property(e => e.TaxPercentage).HasPrecision(5, 2).HasColumnName("taxpercentage");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.MinQuantity).HasColumnName("minquantity");
                entity.Property(e => e.CreatedDate).HasColumnName("createddate").HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedDate).HasColumnName("updateddate").HasDefaultValueSql("NOW()");
                entity.Property(e => e.IsActive).HasColumnName("isactive").HasDefaultValue(true);
                entity.Property(e => e.CategoryId).HasColumnName("category_id");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("items_category_id_fkey");

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.Items) // Assumes a Supplier can have many Items
                    .HasForeignKey(d => d.SupplierId)
                    .HasConstraintName("fk_supplier"); // The constraint name we created
            });

            // --------- ItemHistories Table Mapping ----------
            modelBuilder.Entity<ItemHistory>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("itemhistories_pkey");

                entity.ToTable("itemhistories");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ItemId).HasColumnName("itemid");
                entity.Property(e => e.TransactionType).HasColumnName("transactiontype");
                entity.Property(e => e.QuantityChanged).HasColumnName("quantitychanged");
                entity.Property(e => e.Remarks).HasColumnName("remarks");
                entity.Property(e => e.Timestamp).HasColumnName("timestamp").HasDefaultValueSql("NOW()");
                entity.Property(e => e.UserName).HasColumnName("username");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ItemHistories)
                    .HasForeignKey(d => d.ItemId)
                    .HasConstraintName("fk_item");
            });
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.ToTable("sales");

                entity.HasKey(e => e.SaleId);

                // Map property names to column names
                // Make sure these names exactly match your database columns
                entity.Property(e => e.SaleId).HasColumnName("sale_id");
                entity.Property(e => e.ItemId).HasColumnName("item_id");
                entity.Property(e => e.QuantitySold).HasColumnName("quantity_sold");
                entity.Property(e => e.SalePriceAtTimeOfSale).HasColumnName("sale_price_at_time_of_sale");
                entity.Property(e => e.CostPriceAtTimeOfSale).HasColumnName("cost_price_at_time_of_sale");
                entity.Property(e => e.SaleTimestamp).HasColumnName("sale_timestamp");
            });

            // --------- Suppliers Table Mapping ----------
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("suppliers_pkey");

                entity.ToTable("suppliers");

                // Map class properties to database column names
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20)
                    .HasColumnName("phone_number");

                entity.Property(e => e.Website)
                    .HasMaxLength(255)
                    .HasColumnName("website");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .HasColumnName("email");

                entity.Property(e => e.ContactPerson)
                    .HasMaxLength(255)
                    .HasColumnName("contact_person");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("NOW()");

                entity.Property(e => e.WhatsAppNumber)
                .HasMaxLength(20)
                .HasColumnName("whatsapp_number");
            });

            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.ToTable("purchase_orders");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
                entity.Property(e => e.OrderDate).HasColumnName("order_date");
                entity.Property(e => e.ExpectedDate).HasColumnName("expected_date");

                entity.HasOne(d => d.Supplier)
                    .WithMany() // Assuming a supplier can have many purchase orders
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_supplier");
            });


            // --------- PurchaseOrderItems Table Mapping ----------
            modelBuilder.Entity<PurchaseOrderItem>(entity =>
            {
                entity.ToTable("purchase_order_items");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");
                entity.Property(e => e.ItemId).HasColumnName("item_id");
                entity.Property(e => e.QuantityOrdered).HasColumnName("quantity_ordered");

                entity.HasOne(d => d.PurchaseOrder)
                    .WithMany(p => p.PurchaseOrderItems)
                    .HasForeignKey(d => d.PurchaseOrderId)
                    .HasConstraintName("fk_purchase_order");

                entity.HasOne(d => d.Item)
                    .WithMany() // Assuming an item can be in many purchase order lines
                    .HasForeignKey(d => d.ItemId)
                    .HasConstraintName("fk_item");
            });



            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
