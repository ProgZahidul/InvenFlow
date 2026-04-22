using InventoryManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels
{
        public class GRNItemViewModel
        {
            public int PurchaseOrderItemId { get; set; }
            public int ItemId { get; set; }
            public string ItemName { get; set; }
            public int QuantityOrdered { get; set; }

            [Range(0, int.MaxValue)]
            public int QuantityReceived { get; set; }

            public decimal UnitPrice { get; set; }

            [Required]
            public string BatchNumber { get; set; }

            [Required]
            public DateTime? ExpiryDate { get; set; }
        }

        // ViewModels/StockViewModels.cs
}
