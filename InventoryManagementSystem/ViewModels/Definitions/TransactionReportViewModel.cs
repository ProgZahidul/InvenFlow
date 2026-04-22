using InventoryManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels
{
        public class TransactionReportViewModel
        {
            public DateTime Date { get; set; }
            public string Type { get; set; }
            public string ItemName { get; set; }
            public int Quantity { get; set; }
            public string Reference { get; set; }
            public decimal UnitCost { get; set; }
            public decimal TotalCost { get; set; }
        }
}
