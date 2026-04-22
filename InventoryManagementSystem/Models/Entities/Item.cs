using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
        public class Item
        {
            public int Id { get; set; }

            [Required, StringLength(100)]
            public string Name { get; set; }

            [StringLength(500)]
            public string Description { get; set; }

            [Required, StringLength(50)]
            public string Code { get; set; }

            [Required]
            public int UnitOfMeasureId { get; set; }
            public UnitOfMeasure UnitOfMeasure { get; set; }

            [Range(0, double.MaxValue)]
            public decimal? UnitPrice { get; set; }

            [Range(0, int.MaxValue)]
            public int ReorderLevel { get; set; } = 10;

            public int? CategoryId { get; set; }
            public Category Category { get; set; }

            public bool IsActive { get; set; } = true;

            public ICollection<RequisitionItem> RequisitionItems { get; set; }
            public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; }
            public ICollection<Stock> Stocks { get; set; }
            public ICollection<IssueItem> IssueItems { get; set; }
        }
}
