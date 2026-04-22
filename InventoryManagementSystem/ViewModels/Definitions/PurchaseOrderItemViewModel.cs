using InventoryManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels
{
        public class PurchaseOrderItemViewModel
        {
            public int ItemId { get; set; }
            public string ItemName { get; set; }

            [Range(1, int.MaxValue)]
            public int QuantityOrdered { get; set; }

            [Range(0.01, double.MaxValue)]
            public decimal UnitPrice { get; set; }
        }

        // ViewModels/GRNViewModels.cs
}
