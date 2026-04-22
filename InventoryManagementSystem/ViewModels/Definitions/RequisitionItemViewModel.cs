using InventoryManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels
{
        public class RequisitionItemViewModel
        {
            public int ItemId { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
            public int Quantity { get; set; }

            public string Notes { get; set; }
        }

        // ViewModels/PurchaseOrderViewModels.cs
}
