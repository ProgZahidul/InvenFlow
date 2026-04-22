using InventoryManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels
{
        public class StockReportViewModel
        {
            public string ItemName { get; set; }
            public string ItemCode { get; set; }
            public string UnitOfMeasure { get; set; }
            public int TotalQuantity { get; set; }
            public int AvailableQuantity { get; set; }
            public int ReservedQuantity { get; set; }
            public int ReorderLevel { get; set; }
        }
}
