using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
        public class Requisition
        {
            public int Id { get; set; }

            [Required, StringLength(50)]
            public string RequisitionNumber { get; set; }

            [Required, StringLength(200)]
            public string Title { get; set; }

            [StringLength(1000)]
            public string Description { get; set; }

            public int DepartmentId { get; set; }
            public Department Department { get; set; }

            [Required]
            public string RequestedById { get; set; }
            public ApplicationUser RequestedBy { get; set; }

            public DateTime RequestedDate { get; set; } = DateTime.UtcNow;

            public RequisitionStatus Status { get; set; } = RequisitionStatus.Pending;

            [StringLength(1000)]
            public string? StatusNotes { get; set; }

            public string? ApprovedById { get; set; }
            public ApplicationUser? ApprovedBy { get; set; }

            public DateTime? ApprovedDate { get; set; }

            public ICollection<RequisitionItem> RequisitionItems { get; set; }
            public ICollection<PurchaseOrder> PurchaseOrders { get; set; }
        }
}
