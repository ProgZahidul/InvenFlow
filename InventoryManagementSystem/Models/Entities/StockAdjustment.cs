using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
        public class StockAdjustment
        {
            public int Id { get; set; }

            public int StockId { get; set; }
            public Stock Stock { get; set; }

            public int PreviousQuantity { get; set; }
            public int AdjustmentQuantity { get; set; }
            public int NewQuantity { get; set; }

            [Required, StringLength(500)]
            public string Reason { get; set; }

            public string AdjustedById { get; set; }
            public ApplicationUser AdjustedBy { get; set; }

            public DateTime AdjustedDate { get; set; } = DateTime.UtcNow;
        }
}
