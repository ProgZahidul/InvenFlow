using InventoryManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Department> Departments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Requisition> Requisitions { get; set; }
        public DbSet<RequisitionItem> RequisitionItems { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<GoodsReceivedNote> GoodsReceivedNotes { get; set; }
        public DbSet<GRNItem> GRNItems { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<StockAdjustment> StockAdjustments { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<IssueItem> IssueItems { get; set; }
        public DbSet<UnitOfMeasure> UnitOfMeasures { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ApplicationUser relationships
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Requisition relationships
            builder.Entity<Requisition>()
                .HasOne(r => r.RequestedBy)
                .WithMany(u => u.Requisitions)
                .HasForeignKey(r => r.RequestedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Requisition>()
                .HasOne(r => r.ApprovedBy)
                .WithMany()
                .HasForeignKey(r => r.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Requisition>()
                .HasOne(r => r.Department)
                .WithMany(d => d.Requisitions)
                .HasForeignKey(r => r.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // PurchaseOrder relationships
            builder.Entity<PurchaseOrder>()
                .HasOne(p => p.CreatedBy)
                .WithMany(u => u.PurchaseOrdersCreated)
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PurchaseOrder>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.PurchaseOrders)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PurchaseOrder>()
                .HasOne(p => p.Requisition)
                .WithMany(r => r.PurchaseOrders)
                .HasForeignKey(p => p.RequisitionId)
                .OnDelete(DeleteBehavior.Restrict);

            // PurchaseOrderItem relationships
            builder.Entity<PurchaseOrderItem>()
                .HasOne(p => p.PurchaseOrder)
                .WithMany(po => po.PurchaseOrderItems)
                .HasForeignKey(p => p.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade); // Changed to Cascade

            builder.Entity<PurchaseOrderItem>()
                .HasOne(p => p.Item)
                .WithMany(i => i.PurchaseOrderItems)
                .HasForeignKey(p => p.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // GRN relationships
            builder.Entity<GoodsReceivedNote>()
                .HasOne(g => g.ReceivedBy)
                .WithMany(u => u.GoodsReceivedNotes)
                .HasForeignKey(g => g.ReceivedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GoodsReceivedNote>()
                .HasOne(g => g.PurchaseOrder)
                .WithMany(po => po.GoodsReceivedNotes)
                .HasForeignKey(g => g.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // GRNItem relationships
            builder.Entity<GRNItem>()
                .HasOne(g => g.GoodsReceivedNote)
                .WithMany(grn => grn.GRNItems)
                .HasForeignKey(g => g.GRNId)
                .OnDelete(DeleteBehavior.Cascade); // Changed to Cascade

            builder.Entity<GRNItem>()
                .HasOne(g => g.PurchaseOrderItem)
                .WithMany(poi => poi.GRNItems)
                .HasForeignKey(g => g.PurchaseOrderItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Stock relationships
            builder.Entity<Stock>()
                .HasOne(s => s.Item)
                .WithMany(i => i.Stocks)
                .HasForeignKey(s => s.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Stock>()
                .HasOne(s => s.GRNItem)
                .WithMany(gi => gi.Stocks)
                .HasForeignKey(s => s.GRNItemId)
                .OnDelete(DeleteBehavior.Cascade); // Changed to Cascade

            // Issue relationships
            builder.Entity<Issue>()
                .HasOne(i => i.RequestedBy)
                .WithMany(u => u.IssuesRequested)
                .HasForeignKey(i => i.RequestedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Issue>()
                .HasOne(i => i.IssuedBy)
                .WithMany(u => u.IssuesIssued)
                .HasForeignKey(i => i.IssuedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Issue>()
                .HasOne(i => i.Department)
                .WithMany(d => d.Issues)
                .HasForeignKey(i => i.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // StockAdjustment relationships
            builder.Entity<StockAdjustment>()
                .HasOne(s => s.AdjustedBy)
                .WithMany(u => u.StockAdjustments)
                .HasForeignKey(s => s.AdjustedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StockAdjustment>()
                .HasOne(s => s.Stock)
                .WithMany(st => st.StockAdjustments)
                .HasForeignKey(s => s.StockId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure decimal precision
            builder.Entity<Item>()
                .Property(i => i.UnitPrice)
                .HasPrecision(18, 2);

            builder.Entity<PurchaseOrderItem>()
                .Property(poi => poi.UnitPrice)
                .HasPrecision(18, 2);

            builder.Entity<GRNItem>()
                .Property(gi => gi.UnitCost)
                .HasPrecision(18, 2);

            builder.Entity<Stock>()
                .Property(s => s.UnitCost)
                .HasPrecision(18, 2);

            builder.Entity<PurchaseOrder>()
                .Property(po => po.TotalAmount)
                .HasPrecision(18, 2);

            // Configure indexes
            builder.Entity<Requisition>()
                .HasIndex(r => r.RequisitionNumber)
                .IsUnique();

            builder.Entity<PurchaseOrder>()
                .HasIndex(po => po.PONumber)
                .IsUnique();

            builder.Entity<GoodsReceivedNote>()
                .HasIndex(grn => grn.GRNNumber)
                .IsUnique();

            builder.Entity<Issue>()
                .HasIndex(i => i.IssueNumber)
                .IsUnique();

            builder.Entity<Item>()
                .HasIndex(i => i.Code)
                .IsUnique();

            builder.Entity<Department>()
                .HasIndex(d => d.Code)
                .IsUnique();
        }
    }
}
