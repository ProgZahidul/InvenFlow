using InventoryManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels
{
        public class ExpiryReportViewModel
        {
            public string ItemName { get; set; }
            public string BatchNumber { get; set; }
            public DateTime ExpiryDate { get; set; }
            public int Quantity { get; set; }
            public int DaysUntilExpiry { get; set; }
        }
}
