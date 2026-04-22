using InventoryManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels
{
        public class StockAdjustmentViewModel
        {
            public int StockId { get; set; }
            public string ItemName { get; set; }
            public string BatchNumber { get; set; }
            public int CurrentQuantity { get; set; }

            [Required]
            public int AdjustmentQuantity { get; set; }

            [Required]
            public string Reason { get; set; }
        }
}
