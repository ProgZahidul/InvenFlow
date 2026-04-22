using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
        public class ApplicationUser : IdentityUser
        {
            [Required, StringLength(50)]
            public string FirstName { get; set; }

            [Required, StringLength(50)]
            public string LastName { get; set; }

            public int? DepartmentId { get; set; }
            public Department Department { get; set; }

            // Add this property
            public bool IsActive { get; set; } = true;

            // Navigation properties
            public ICollection<Requisition> Requisitions { get; set; }
            public ICollection<PurchaseOrder> PurchaseOrdersCreated { get; set; }
            public ICollection<GoodsReceivedNote> GoodsReceivedNotes { get; set; }
            public ICollection<Issue> IssuesRequested { get; set; }
            public ICollection<Issue> IssuesIssued { get; set; }
            public ICollection<StockAdjustment> StockAdjustments { get; set; }
        }
}
