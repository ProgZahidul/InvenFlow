using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
        public class Stock
        {
            public int Id { get; set; }

            public int ItemId { get; set; }
            public Item Item { get; set; }

            public int GRNItemId { get; set; }
            public GRNItem GRNItem { get; set; }

            [Required, StringLength(50)]
            public string BatchNumber { get; set; }

            public DateTime? ExpiryDate { get; set; }

            [Range(0.01, double.MaxValue)]
            public decimal UnitCost { get; set; }

            [Range(0, int.MaxValue)]
            public int Quantity { get; set; }

            [Range(0, int.MaxValue)]
            public int ReservedQuantity { get; set; }

            [NotMapped]
            public int AvailableQuantity => Quantity - ReservedQuantity;

            public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

            public ICollection<IssueItem> IssueItems { get; set; }
            public ICollection<StockAdjustment> StockAdjustments { get; set; }
        }
}
