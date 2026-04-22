using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
        public class PurchaseOrder
        {
            public int Id { get; set; }

            [Required, StringLength(50)]
            public string PONumber { get; set; }

            public int RequisitionId { get; set; }
            public Requisition Requisition { get; set; }

            public int SupplierId { get; set; }
            public Supplier Supplier { get; set; }

            [Required]
            public string CreatedById { get; set; }
            public ApplicationUser CreatedBy { get; set; }

            public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
            public DateTime? ExpectedDeliveryDate { get; set; }
            public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Pending;

            [StringLength(1000)]
            public string StatusNotes { get; set; }

            [Range(0, double.MaxValue)]
            public decimal TotalAmount { get; set; }

            public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; }
            public ICollection<GoodsReceivedNote> GoodsReceivedNotes { get; set; }
        }
}
