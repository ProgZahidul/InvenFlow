using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
        public class GoodsReceivedNote
        {
            public int Id { get; set; }

            [Required, StringLength(50)]
            public string GRNNumber { get; set; }

            public int PurchaseOrderId { get; set; }
            public PurchaseOrder PurchaseOrder { get; set; }

            [Required]
            public string ReceivedById { get; set; }
            public ApplicationUser ReceivedBy { get; set; }

            public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;

            [StringLength(1000)]
            public string Notes { get; set; }

            public ICollection<GRNItem> GRNItems { get; set; }
        }
}
